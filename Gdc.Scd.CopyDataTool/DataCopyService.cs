using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Ninject;

namespace Gdc.Scd.CopyDataTool
{
    public class DataCopyService
    {
        private readonly DomainEnitiesMeta meta;
        private readonly IKernel kernel;
        private readonly CopyDetailsConfig config;
        private readonly Dictionary<string, Dictionary<string, long>> _dependencies;
        private readonly ISqlRepository _sqlRepository;
        private List<EditInfo> _approvedEditInfos;
        private List<EditInfo> _toBeApprovedEditInfos;

        public DataCopyService()
        {
            NinjectExt.IsConsoleApplication = true;
            kernel = CreateKernel();
            meta = kernel.Get<DomainEnitiesMeta>();
            config = kernel.Get<CopyDetailsConfig>();
            _sqlRepository = kernel.Get<SqlRepository>();
            _dependencies = LoadDependencies(meta);
            _approvedEditInfos = new List<EditInfo>();
            _toBeApprovedEditInfos = new List<EditInfo>();
        }

        public void CopyData()
        {
            var costBlocks = this.meta.CostBlocks
                .Where(costBlock =>
                    config.CostBlocks.Cast<CostBlockElement>()
                        .Any(cb => cb.Name == costBlock.Name && cb.Schema == costBlock.Schema)).ToArray();

            // Не во всех костблоках есть страна. 
            if (string.IsNullOrEmpty(this.config.Country))
            {
                costBlocks = 
                    costBlocks.Where(costBlock => costBlock.InputLevelFields[MetaConstants.CountryInputLevelName] != null)
                              .ToArray();
            }

            var sourceResult = GetSourceData(costBlocks);
            GetEditInfos(sourceResult, costBlocks);
        }

        private Dictionary<string, Dictionary<string, long>> LoadDependencies(DomainEnitiesMeta meta)
        {
            var dependencies = meta.Dependencies.Concat(meta.InputLevels);
            var result = new Dictionary<string, Dictionary<string, long>>();
            foreach (var dependency in dependencies)
            {
                if (result.ContainsKey(dependency.Name))
                    continue;

                var entitiesTask = _sqlRepository.GetNameIdItems(dependency, dependency.IdField.Name, dependency.NameField.Name);

                entitiesTask.Wait();

                if (entitiesTask.Result != null && entitiesTask.Result.Any())
                    result[dependency.Name] = entitiesTask.Result.ToDictionary(e => e.Name, e => e.Id);

            }

            return result;
        }

        private List<DataTable> GetSourceData(CostBlockEntityMeta[] costBlocks)
        {
            var sourceResult = new List<DataTable>();

            var efRepoSet = new EntityFrameworkRepositorySet(kernel, "SourceDB");

            foreach (var costBlock in costBlocks)
            {
                var joinInfos = costBlock.CoordinateFields.Select(cf => new JoinInfo(costBlock, cf.Name));
                var columnInfos = costBlock.CostElementsFields.Select(cef => new ColumnInfo(cef.Name, costBlock.Name));
                var columnInfosArr = columnInfos.Concat(costBlock.CoordinateFields.Select(c =>
                        new ColumnInfo(c.ReferenceFaceField, c.ReferenceMeta.Name, c.ReferenceMeta.Name)))
                    .Concat(costBlock.CostElementsApprovedFields.Values.Select(c => 
                        new ColumnInfo(c.Name, costBlock.Name)
                        )).Concat(new List<ColumnInfo>{new ColumnInfo(MetaConstants.IdFieldKey, costBlock.Name) })
                    .ToArray();

                var selectCostBlockValueQuery =
                    Sql.Select(columnInfosArr).From(costBlock)
                        .Join(joinInfos)
                        .Where(new Dictionary<string, IEnumerable<object>>
                                { [MetaConstants.NameFieldKey] = new object[] { config.Country } },
                            MetaConstants.CountryInputLevelName);


                var task = efRepoSet.ReadBySql(selectCostBlockValueQuery, costBlock.Name);

                task.Wait();

                sourceResult.Add(task.Result);
            }

            return sourceResult;
        }

        private void GetEditInfos(List<DataTable> source, CostBlockEntityMeta[] costBlocks)
        {
            foreach (var costBlock in costBlocks)
            {
                var editInfoApproved = new EditInfo
                {
                    Meta = costBlock
                };

                var editInfoToBeApproved = new EditInfo
                {
                    Meta = costBlock
                };

                var valuesInfoApproved = new List<ValuesInfo>();
                var valuesInfoToBeApproved = new List<ValuesInfo>();

                var groupedCostElements = GetGroupedCostElements(costBlock);

                var dataTable = source.FirstOrDefault(table => table.TableName == costBlock.Name);
                if (dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var costElement in groupedCostElements)
                        {
                            var coordinateFilter = BuildCoordinates(costElement.InputLevels, row);

                            var approveValues = new Dictionary<string, object>();
                            var toApproveValues = new Dictionary<string, object>();

                            BuildCostElementValues(costElement.CostElements, row, approveValues, toApproveValues);

                            var valueInfoToApprove = new ValuesInfo
                            {
                                CoordinateFilter = coordinateFilter,
                                Values = approveValues
                            };

                            var valueInfoToBeApprove = new ValuesInfo
                            {
                                CoordinateFilter = coordinateFilter,
                                Values = toApproveValues
                            };

                            if (valueInfoToApprove.Values.Any())
                                valuesInfoApproved.Add(valueInfoToApprove);
                            if (valueInfoToBeApprove.Values.Any())
                                valuesInfoToBeApproved.Add(valueInfoToBeApprove);
                        }
                    }

                    editInfoApproved.ValueInfos = valuesInfoApproved;
                    editInfoToBeApproved.ValueInfos = valuesInfoToBeApproved;

                    _approvedEditInfos.Add(editInfoApproved);
                    _toBeApprovedEditInfos.Add(editInfoToBeApproved);
                }
            }
        }

        private Dictionary<string, long[]> BuildCoordinates(List<string> coordinateNames, DataRow row)
        {
            var result = coordinateNames.ToDictionary(c => c, c => 
                {
                    if (_dependencies.ContainsKey(c))
                    {
                        if (_dependencies[c].ContainsKey(row[c].ToString()))
                            return new [] {_dependencies[c][row[c].ToString()] };
                        throw new Exception("Unknown coordinate: " + row[c].ToString());
                    }

                    throw new Exception("Unknown dependency: " + c);
                });
            return result;
        }

        private void BuildCostElementValues(List<string> costElementFields, DataRow row,
            IDictionary<string, object> approvedValues, 
            IDictionary<string, object> toApproveValues)
        {
            foreach (var costElementField in costElementFields)
            {
                if (row[costElementField].ToString() == row[costElementField + "_Approved"].ToString())
                    approvedValues.Add(costElementField, row[costElementField]);
                else
                {
                    approvedValues.Add(costElementField, row[costElementField + "_Approved"]);
                    toApproveValues.Add(costElementField, row[costElementField]);
                }
            }
        }

        private List<GroupedCostElements> GetGroupedCostElements(CostBlockEntityMeta costBlock)
        {
            var groupedCostElements = new List<GroupedCostElements>();
            var costElements = costBlock.CostElementsFields.Select(ce => ce.Name);
            foreach (var costElement in costElements)
            {
                var coordinateFields = costBlock.GetDomainCoordinateFields(costElement)
                    .Select(c => c.Name).ToList();

                var groupCostElement =
                    groupedCostElements.FirstOrDefault(gc => gc.InputLevels.SequenceEqual(coordinateFields));
                if (groupCostElement != null)
                    groupCostElement.CostElements.Add(costElement);
                else
                {
                    var newGroup = new GroupedCostElements
                    {
                        CostElements = new List<string> {costElement},
                        InputLevels = coordinateFields
                    };
                    groupedCostElements.Add(newGroup);
                }
            }

            return groupedCostElements;
        }

        private static StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Core.Module(),
                new DataAccessLayer.Module(),
                new BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
