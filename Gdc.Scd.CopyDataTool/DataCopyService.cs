using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.CopyDataTool.Entities;
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
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
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
        private readonly ICostBlockService _costBlockService;
        private readonly IApprovalService _approvalService;

        public DataCopyService(IKernel kernel)
        {
            this.kernel = kernel;
            meta = this.kernel.Get<DomainEnitiesMeta>();
            config = this.kernel.Get<CopyDetailsConfig>();
            _sqlRepository = this.kernel.Get<SqlRepository>();
            Console.WriteLine("Load Dependencies...");
            _dependencies = LoadDependencies(meta);
            _approvedEditInfos = new List<EditInfo>();
            _toBeApprovedEditInfos = new List<EditInfo>();
            _costBlockService = this.kernel.Get<ICostBlockService>();
            _approvalService = this.kernel.Get<IApprovalService>();
        }

        public void CopyData()
        {
            var costBlocks = this.meta.CostBlocks
                .Where(costBlock =>
                    config.CostBlocks.Cast<CostBlockElement>()
                        .Any(cb => cb.Name == costBlock.Name && cb.Schema == costBlock.Schema)).ToArray();

            // Not all cost blocks have country 
            if (string.IsNullOrEmpty(this.config.Country))
            {
                costBlocks = 
                    costBlocks.Where(costBlock => costBlock.InputLevelFields[MetaConstants.CountryInputLevelName] != null)
                              .ToArray();
            }

            Console.WriteLine("Get data from source...");
            var sourceResult = GetSourceData(costBlocks);
            Console.WriteLine("Data was received.");
            Console.WriteLine("Build edit infos...");
            GetEditInfos(sourceResult, costBlocks);
            Console.WriteLine("Edit info is built.");
            Console.WriteLine("Starting to update the target...");

            var approvalOption = new ApprovalOption {IsApproving = true, TurnOffNotification = true};

            var updateTask = _costBlockService.UpdateWithoutQualityGate(_approvedEditInfos.ToArray(),
                approvalOption, EditorType.Migration);

            updateTask.Wait();

            var histories = updateTask.Result.ToList();
            Console.WriteLine("Approving...");
            Console.WriteLine($"Approving history count - {histories.Count}");

            var historyIndex = 0;

            foreach (var history in histories)
            {
                Console.WriteLine($"History index: {historyIndex++}/{histories.Count - 1}");

                var approveTask = _approvalService.Approve(history.Id, approvalOption.TurnOffNotification);

                approveTask.Wait();
            }

            Console.WriteLine("Sending for approve values...");
            updateTask = _costBlockService.UpdateWithoutQualityGate(_toBeApprovedEditInfos.ToArray(),
                new ApprovalOption { IsApproving = false }, EditorType.Migration);

            updateTask.Wait();
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
                    )).Concat(new List<ColumnInfo> {new ColumnInfo(MetaConstants.IdFieldKey, costBlock.Name)})
                    .ToArray();

                var whereConditionHelper = ConditionHelper.AndStatic(
                        new Dictionary<string, IEnumerable<object>>
                        {
                            [MetaConstants.NameFieldKey] = new object[] {config.Country},
                        },
                        MetaConstants.CountryInputLevelName)
                    .And(CostBlockQueryHelper.BuildNotDeletedCondition(costBlock, costBlock.Name));

                if (costBlock.InputLevelFields[MetaConstants.WgInputLevelName] != null)
                {
                    var wgMetaInfo = costBlock.InputLevelFields[MetaConstants.WgInputLevelName];

                    whereConditionHelper = whereConditionHelper.And(new NotInSqlBuilder
                    {
                        Column = wgMetaInfo.ReferenceFaceField,
                        Table = wgMetaInfo.Name,
                        Values = config.ExcludedWgs.Split(',').Select(wg => new ValueSqlBuilder(wg))
                    });
                }


                var selectCostBlockValueQuery =
                    Sql.Select(columnInfosArr).From(costBlock)
                        .Join(joinInfos)
                        .Where(whereConditionHelper);



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

                var cache = new Cache();

                if (dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var costElement in groupedCostElements)
                        {
                            var coordinateFilter = BuildCoordinates(costElement.Coordinates, row);

                            var approveValues = new Dictionary<string, object>();
                            var toApproveValues = new Dictionary<string, object>();

                            var requireAdjustments = !BuildCostElementValues(costElement.CostElements, 
                                row, approveValues, toApproveValues,
                                cache, coordinateFilter);


                            var valueInfoToApprove = new ValuesInfo
                            {
                                CoordinateFilter = coordinateFilter.Convert(),
                                Values = approveValues
                            };

                            var valueInfoToBeApprove = new ValuesInfo
                            {
                                CoordinateFilter = coordinateFilter.Convert(),
                                Values = toApproveValues
                            };

                            if (requireAdjustments)
                            {
                                ResolveInconsistency(valueInfoToApprove, valuesInfoApproved);
                                ResolveInconsistency(valueInfoToBeApprove, valuesInfoToBeApproved);
                            }

                            else
                            {

                                if (valueInfoToApprove.Values.Any())
                                    valuesInfoApproved.Add(valueInfoToApprove);
                                if (valueInfoToBeApprove.Values.Any())
                                    valuesInfoToBeApproved.Add(valueInfoToBeApprove);
                            }
                        }
                    }

                    editInfoApproved.ValueInfos = valuesInfoApproved;
                    editInfoToBeApproved.ValueInfos = valuesInfoToBeApproved;

                    _approvedEditInfos.Add(editInfoApproved);
                    _toBeApprovedEditInfos.Add(editInfoToBeApproved);
                }
            }
        }

        private Dictionary<string, long> BuildCoordinates(List<string> coordinateNames, DataRow row)
        {
            var result = coordinateNames.ToDictionary(c => c, c => 
                {
                    if (_dependencies.ContainsKey(c))
                    {
                        if (_dependencies[c].ContainsKey(row[c].ToString()))
                            return _dependencies[c][row[c].ToString()];
                        throw new Exception("Unknown coordinate: " + row[c].ToString());
                    }

                    throw new Exception("Unknown dependency: " + c);
                });
            return result;
        }

        private bool BuildCostElementValues(List<string> costElementFields, DataRow row,
            IDictionary<string, object> approvedValues, 
            IDictionary<string, object> toApproveValues,
            Cache cache, Dictionary<string, long> coordinates)
        {
            var result = true;

            foreach (var costElementField in costElementFields)
            {
                if (row[costElementField].ToString() == row[costElementField + "_Approved"].ToString())
                {
                    if (!cache.IfExists(costElementField, coordinates, row[costElementField], ApproveSet.Approved))
                    {
                        var isInconsistent = !cache.Add(costElementField, coordinates, row[costElementField], ApproveSet.Approved);
                        approvedValues.Add(costElementField, row[costElementField]);

                        if (isInconsistent)
                            result = false;
                    }
                    
                }
                else
                {
                    if (!cache.IfExists(costElementField, coordinates, row[costElementField + "_Approved"],
                        ApproveSet.Approved))
                    {
                        approvedValues.Add(costElementField, row[costElementField + "_Approved"]);
                        var isInconsistent = !cache.Add(costElementField, coordinates, row[costElementField + "_Approved"],
                            ApproveSet.Approved);

                        if (isInconsistent)
                            result = false;
                    }

                    if (!cache.IfExists(costElementField, coordinates, row[costElementField],
                        ApproveSet.ToBeApproved))
                    {
                        toApproveValues.Add(costElementField, row[costElementField]);
                        var isInconsistent = !cache.Add(costElementField, coordinates, row[costElementField],
                            ApproveSet.ToBeApproved);

                        if (isInconsistent)
                            result = false;
                    }
                }
            }

            return result;
        }

        private List<GroupedCostElements> GetGroupedCostElements(CostBlockEntityMeta costBlock)
        {
            var groupedCostElements = new List<GroupedCostElements>();

            var excludedCostElements = config.ExcludedCostElements.Cast<ExcludedCostElement>()
                                             .Where(ece => ece.CostBlock.Equals(costBlock.Name, StringComparison.OrdinalIgnoreCase))
                                             .Select(ece => ece.Name)
                                             .ToList();


            var costElements = costBlock.CostElementsFields
                .Where(ce => !excludedCostElements.Contains(ce.Name))
                .Select(ce => ce.Name);

            foreach (var costElement in costElements)
            {
                var coordinateFields = costBlock.GetDomainCoordinateFields(costElement)
                    .Select(c => c.Name).ToList();

                var groupCostElement =
                    groupedCostElements.FirstOrDefault(gc => gc.Coordinates.SequenceEqual(coordinateFields));
                if (groupCostElement != null)
                    groupCostElement.CostElements.Add(costElement);
                else
                {
                    var newGroup = new GroupedCostElements
                    {
                        CostElements = new List<string> {costElement},
                        Coordinates = coordinateFields
                    };
                    groupedCostElements.Add(newGroup);
                }
            }

            return groupedCostElements;
        }

        private void ResolveInconsistency(ValuesInfo valuesInfo, List<ValuesInfo> valuesInfos)
        {
            var updatedCoordinates = valuesInfo.CoordinateFilter.Select(c => string.Join(",", $"{c.Key}-{c.Value[0]}"));

            var updatedCoordinate = String.Join(",", updatedCoordinates);            

            ValuesInfo inconsistent = null;
            foreach (var vi in valuesInfos)
            {
                var coordinates = vi.CoordinateFilter.Select(c => string.Join(",", $"{c.Key}-{c.Value[0]}"));

                var concatenatedCoordinates = String.Join(",", coordinates);

                if (concatenatedCoordinates == updatedCoordinate)
                {
                    inconsistent = vi;
                    break;
                }
            }

            if (inconsistent != null)
            {
                foreach (var val in valuesInfo.Values)
                {
                    var valInUpdatedValuesInfo = inconsistent.Values.First(v => v.Key == val.Key);
                    inconsistent.Values[val.Key] = val.Value.ToString() == String.Empty ? valInUpdatedValuesInfo.Value : val.Value;
                }
            }
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
