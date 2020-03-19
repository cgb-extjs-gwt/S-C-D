using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool
{
    public class DataCopyService
    {
        private readonly DomainEnitiesMeta meta;
        private readonly IKernel kernel;
        private readonly CopyDetailsConfig config;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICostBlockService _costBlockService;
        private readonly IUserService userService;

        public DataCopyService(IKernel kernel)
        {
            this.kernel = kernel;
            this.userService = kernel.Get<IUserService>();
            meta = this.kernel.Get<DomainEnitiesMeta>();
            config = this.kernel.Get<CopyDetailsConfig>();
            _sqlRepository = this.kernel.Get<SqlRepository>();
            _costBlockService = this.kernel.Get<ICostBlockService>();
        }

        public void CopyData()
        {
            if (this.config.CopyCostBlocks)
            {
                var costBlocks = (IEnumerable<CostBlockEntityMeta>)this.meta.CostBlocks;

                var costBlockElements = this.config.CostBlocks.Cast<CostBlockElement>().ToArray();
                if (costBlockElements.Length > 0)
                {
                    costBlocks =
                        costBlocks.Where(
                            costBlock => costBlockElements.Any(
                                costBlockEl => costBlock.Name == costBlockEl.Name && costBlock.Schema == costBlockEl.Schema));
                }

                this.CopyCostBlocks(costBlocks.ToArray());
            }
        }

        private IEnumerable<CostBlockDataInfo> GetDataInfo(IEnumerable<CostBlockEntityMeta> costBlocks)
        {
            var sourceRepositorySet = new EntityFrameworkRepositorySet(kernel, "SourceDB");
            var targetRepositorySet = new EntityFrameworkRepositorySet(kernel, "CommonDB");
            var excludedCostElements = config.ExcludedCostElements.Cast<ExcludedCostElement>().ToArray();

            var costBlockInfos = costBlocks.SelectMany(
                block =>
                    block.DomainMeta.CostElements.Where(elem => !excludedCostElements.Any(x => x.CostBlock == block.Name && x.Name == elem.Id))
                                                 .Select(elem => new
                                                 {
                                                     CostBlock = block,
                                                     CostElement = elem,
                                                     CoordinateIds =
                                                        elem.Coordinates.Select(coord => coord.Id)
                                                                        .OrderBy(x => x)
                                                                        .ToArray()
                                                 }));

            if (this.config.HasCountry)
            {
                costBlockInfos = costBlockInfos.Where(info => info.CoordinateIds.Contains(MetaConstants.CountryInputLevelName));
            }

            var costBlockGroups = costBlockInfos.GroupBy(info => new
            {
                info.CostBlock,
                Coordinates = string.Join(",", info.CoordinateIds),
            });

            var excludedWgNames = config.GetExcludedWgs();

            foreach (var costBlockGroup in costBlockGroups)
            {
                var costBlock = costBlockGroup.Key.CostBlock;
                var costElementIds = costBlockGroup.Select(info => info.CostElement.Id).ToArray();
                var firstGroup = costBlockGroup.First();

                var sourceQuery = BuildQuery(costBlock, costElementIds, firstGroup.CoordinateIds, this.config.Country, this.config.TargetCountry);
                var sourceTask = sourceRepositorySet.ReadBySql(sourceQuery, costBlock.Name);
                var targetQuery = BuildQuery(costBlock, costElementIds, firstGroup.CoordinateIds, this.config.TargetCountry ?? this.config.Country);
                var targetTask = targetRepositorySet.ReadBySql(targetQuery, costBlock.Name);

                Task.WaitAll(sourceTask, targetTask);

                var sourceRows = sourceTask.Result.Rows.Cast<DataRow>();
                var targetRows = targetTask.Result.Rows.Cast<DataRow>();
                var costElementFields = 
                    costElementIds.Concat(costElementIds.Select(costElementId => costBlock.GetApprovedCostElement(costElementId).Name))
                                  .ToArray();

                var joinedRows =
                    sourceRows.AsParallel()
                              .Join(
                                targetRows.AsParallel(),
                                row => this.GetKey(row, firstGroup.CoordinateIds),
                                row => this.GetKey(row, firstGroup.CoordinateIds),
                                (sourceRow, targetRow) => new { SourceRow = sourceRow, TargetRow = targetRow })
                              .ToArray();

                foreach (var costElementId in costElementIds)
                {
                    var approvedRows = new List<DataRow>();
                    var notApprovedRows = new List<DataRow>();
                    var costElementApprovedId = costBlock.GetApprovedCostElement(costElementId).Name;

                    foreach (var joinedRow in joinedRows)
                    {
                        var sourceApprovedValue = joinedRow.SourceRow[costElementApprovedId];
                        var sourceNotApprovedValue = joinedRow.SourceRow[costElementId];
                        var targetApprovedValue = joinedRow.TargetRow[costElementApprovedId];
                        var targetNotApprovedValue = joinedRow.TargetRow[costElementId];

                        if (!sourceApprovedValue.Equals(targetApprovedValue) ||
                            !sourceNotApprovedValue.Equals(targetNotApprovedValue))
                        {
                            if (sourceNotApprovedValue.Equals(sourceApprovedValue))
                            {
                                approvedRows.Add(joinedRow.SourceRow);
                            }
                            else
                            {
                                approvedRows.Add(joinedRow.SourceRow);
                                notApprovedRows.Add(joinedRow.SourceRow);
                            }
                        }
                    }

                    yield return new CostBlockDataInfo
                    {
                        Meta = costBlock,
                        CostElementId = costElementId,
                        ApprovedRows = approvedRows.ToArray(),
                        NotApprovedRows = notApprovedRows.ToArray(),
                        SourceRowCount = sourceTask.Result.Rows.Count,
                        TargetRowCount = targetTask.Result.Rows.Count,
                        DependencyId = firstGroup.CostElement.Dependency?.Id,
                        InputLevelIds = firstGroup.CostElement.InputLevels.Select(x => x.Id).ToArray(),
                        CoordinateIds = firstGroup.CoordinateIds
                    };
                }
            }

            SqlHelper BuildQuery(
                CostBlockEntityMeta costBlock, 
                string[] сostElementIds, 
                string[] coordinateIds,
                string filterCountry = null,
                string replaceCountry = null)
            {
                var groupByColumns = coordinateIds.Select(coord => new ColumnInfo(MetaConstants.NameFieldKey, coord, coord)).ToArray();
                var costElementColumns =
                    сostElementIds.SelectMany(costElementId =>
                                   {
                                       var costElementField = costBlock.CostElementsFields[costElementId];
                                       var approvedCostElementField = costBlock.GetApprovedCostElement(costElementId);

                                       return new BaseColumnInfo[]
                                       {
                                            SqlFunctions.Min(costElementField, costBlock.Name, costElementId),
                                            SqlFunctions.Min(approvedCostElementField, costBlock.Name, approvedCostElementField.Name)
                                       };
                                   })
                                  .ToArray();

                IEnumerable<BaseColumnInfo> partSelectColumns = groupByColumns;

                if (replaceCountry != null)
                {
                    partSelectColumns =
                        partSelectColumns.Select(
                            column =>
                                column.Alias == MetaConstants.CountryInputLevelName
                                    ? new QueryColumnInfo(new ValueSqlBuilder(replaceCountry), column.Alias)
                                    : column);
                }

                var selectColumns = costElementColumns.Concat(partSelectColumns).ToArray();
                var joinInfos = coordinateIds.Select(coord => new JoinInfo(costBlock, coord)).ToArray();
                var whereCondition = CostBlockQueryHelper.BuildNotDeletedCondition(costBlock, costBlock.Name);

                if (filterCountry != null)
                {
                    whereCondition =
                        whereCondition.And(
                            SqlOperators.Equals(MetaConstants.NameFieldKey, filterCountry, MetaConstants.CountryInputLevelName));
                }

                if (excludedWgNames.Length > 0 &&
                    costBlock.ContainsCoordinateField(MetaConstants.WgInputLevelName))
                {
                    whereCondition = whereCondition.And(new NotInSqlBuilder
                    {
                        Column = MetaConstants.NameFieldKey,
                        Table = MetaConstants.WgInputLevelName,
                        Values = excludedWgNames.Select(wg => new ParameterSqlBuilder(wg))
                    });
                }

                return
                    Sql.Select(selectColumns)
                       .From(costBlock)
                       .Join(joinInfos)
                       .Where(whereCondition)
                       .GroupBy(groupByColumns);
            }
        }

        private string GetKey(DataRow row, string[] columns)
        {
            return string.Join(",", columns.Select(column => row[column].ToString()));
        }

        private (EditInfo[] ApproveEditInfos, EditInfo[] NotApproveEditInfos) GetEditInfos(
            CostBlockDataInfo dataInfo, 
            IDictionary<string, IDictionary<string, long>> coordinateCache)
        {
            var costElementApprovedId = dataInfo.Meta.GetApprovedCostElement(dataInfo.CostElementId).Name;
            var groupByIds = dataInfo.DependencyId == null
                ? dataInfo.InputLevelIds.Skip(1).ToArray()
                : dataInfo.InputLevelIds;

            return
                (
                    BuildEditInfos(dataInfo.ApprovedRows, costElementApprovedId).ToArray(),
                    BuildEditInfos(dataInfo.NotApprovedRows, dataInfo.CostElementId).ToArray()
                );

            ValuesInfo[] GetValuesInfoByGroup(IEnumerable<DataRow> rows, string[] coordinateIds, string valueField)
            {
                return
                    rows.GroupBy(row => row[valueField])
                        .Select(group => new ValuesInfo
                        {
                            CoordinateFilter = GetCoordinateFilter(group, coordinateIds),
                            Values = new Dictionary<string, object>
                            { 
                                [dataInfo.CostElementId] = group.Key
                            }
                        })
                        .ToArray();
            }

            Dictionary<string, long[]> GetCoordinateFilter(IEnumerable<DataRow> rows, string[] coordinateIds)
            {
                return
                    coordinateIds.ToDictionary(
                        coord => coord,
                        coord => 
                            rows.Select(row => row[coord])
                                .Cast<string>()
                                .Distinct()
                                .Select(name => coordinateCache[coord][name])
                                .ToArray());
            }
           
            IEnumerable<EditInfo> BuildEditInfos(IEnumerable<DataRow> rows, string valueField)
            {
                foreach (var rowGroup in rows.GroupBy(row => GetKey(row, groupByIds)))
                {
                    yield return new EditInfo
                    {
                        Meta = dataInfo.Meta,
                        ValueInfos = GetValuesInfoByGroup(rowGroup, dataInfo.CoordinateIds, valueField)
                    };
                }
            }
        }

        private Dictionary<string, IDictionary<string, long>> GetCoordinateCache(IEnumerable<CostBlockEntityMeta> costBlocks)
        {
            var result = new Dictionary<string, IDictionary<string, long>>();

            var metas =
                costBlocks.SelectMany(costBlock => costBlock.CoordinateFields.Select(field => field.ReferenceMeta))
                          .Distinct();

            foreach (var meta in metas)
            {
                var itemsTask = this._sqlRepository.GetNameIdItems(meta, MetaConstants.IdFieldKey, MetaConstants.NameFieldKey);

                itemsTask.Wait();

                result[meta.Name] = itemsTask.Result.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Id);
            }

            return result;
        }

        private void CopyCostBlocks(CostBlockEntityMeta[] costBlocks)
        {
            Console.WriteLine("Coordinate items loadings...");
            var coordinateCache = this.GetCoordinateCache(costBlocks);
            Console.WriteLine("Coordinate items loaded");
            Console.WriteLine();
            Console.WriteLine("Data loading...");

            var currentUser = this.userService.GetCurrentUser();
            var approvalOption = new ApprovalOption
            {
                TurnOffNotification = true
            };

            foreach (var dataInfo in this.GetDataInfo(costBlocks))
            {
                Console.WriteLine($"Data loaded. Cost block: '{dataInfo.Meta.Name}'. Cost element: {dataInfo.CostElementId}");
                Console.WriteLine($"Source row count: {dataInfo.SourceRowCount}. Target row count: {dataInfo.TargetRowCount}");
                Console.WriteLine($"Approve row count: {dataInfo.ApprovedRows.Length}");
                Console.WriteLine($"Not approve row count: {dataInfo.NotApprovedRows.Length}");

                if (dataInfo.ApprovedRows.Length > 0 || dataInfo.NotApprovedRows.Length > 0)
                {
                    Console.WriteLine($"Building EditInfos '{dataInfo.CostElementId}'...");

                    var editInfoSet = this.GetEditInfos(dataInfo, coordinateCache);

                    Console.WriteLine("EditInfos built");

                    if (editInfoSet.ApproveEditInfos.Length > 0)
                    {
                        Console.WriteLine("Update approved values.....");
                        var approvedTask = this._costBlockService.UpdateAsApproved(editInfoSet.ApproveEditInfos, EditorType.Migration, currentUser);
                        approvedTask.Wait();
                        Console.WriteLine("Approved values updated");
                    }

                    if (editInfoSet.NotApproveEditInfos.Length > 0)
                    {
                        Console.WriteLine("Update not approved values.....");
                        var notApprovedTask = this._costBlockService.UpdateWithoutQualityGate(editInfoSet.NotApproveEditInfos, approvalOption, EditorType.Migration, currentUser);
                        notApprovedTask.Wait();
                        Console.WriteLine("Not approved values updated");
                    }
                }
                else
                {
                    Console.WriteLine("No data to copy");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Сopying completed");
        }

        private class CostBlockDataInfo
        {
            public CostBlockEntityMeta Meta { get; set; }

            public string CostElementId { get; set; }

            public DataRow[] ApprovedRows { get; set; }

            public DataRow[] NotApprovedRows { get; set; }

            public int SourceRowCount { get; set; }

            public int TargetRowCount { get; set; }

            public string DependencyId { get; set; }

            public string[] InputLevelIds { get; set; }

            public string[] CoordinateIds { get; set; }
        }
    }
}
