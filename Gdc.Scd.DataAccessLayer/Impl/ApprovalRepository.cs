using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly ICostBlockValueHistoryQueryBuilder historyQueryBuilder;

        private readonly IRepositorySet repositorySet;

        private readonly IQualityGateQueryBuilder qualityGateQueryBuilder;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public ApprovalRepository(
            ICostBlockValueHistoryQueryBuilder historyQueryBuilder, 
            IRepositorySet repositorySet,
            IQualityGateQueryBuilder qualityGateQueryBuilder,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.historyQueryBuilder = historyQueryBuilder;
            this.repositorySet = repositorySet;
            this.qualityGateQueryBuilder = qualityGateQueryBuilder;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<IEnumerable<BundleDetail>> GetApproveBundleDetail(
            CostBlockHistory history,
            long? historyValueId = null,
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.CostBlocks[history.Context];
            var query = BuildQuery();
            
            var maxInputLevelId = this.GetMaxInputLevelId(history, historyValueId);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId, maxInputLevelId)
            {
                UseHistoryValueId = true,
                OldValue = true
            };

            return await this.repositorySet.ReadBySqlAsync(query, mapper.Map);

            SqlHelper BuildQuery()
            {
                const string OldValueName = "OldValue";

                string inputLevelId;
                InputLevelJoinType inputLevelJoinType;
                IEnumerable<ReferenceFieldMeta> domainCoordinateFields;

                var costElementMeta = costBlockMeta.SliceDomainMeta.CostElements[history.Context.CostElementId];

                if (historyValueId.HasValue)
                {
                    var inputLevel = costElementMeta.SortInputLevel().Last();

                    inputLevelId = inputLevel.Id;
                    inputLevelJoinType = InputLevelJoinType.All;
                    domainCoordinateFields = costBlockMeta.GetDomainCoordinateFields(history.Context.CostElementId);
                }
                else
                {
                    inputLevelId = history.Context.InputLevelId;
                    inputLevelJoinType = InputLevelJoinType.HistoryContext;
                    domainCoordinateFields =
                        costElementMeta.SortInputLevel()
                                       .Select(inputLevel => costBlockMeta.InputLevelFields[inputLevel.Id]);

                    var dependencyField = costBlockMeta.GetDomainDependencyField(history.Context.CostElementId);
                    if (dependencyField != null)
                    {
                        domainCoordinateFields = domainCoordinateFields.Concat(new[] { dependencyField });
                    }
                }

                var selectColumns = new List<ColumnInfo>();
                var joinInfos = new List<JoinInfo>();

                var costElementField = costBlockMeta.CostElementsFields[history.Context.CostElementId];
                if (costElementField is ReferenceFieldMeta refCostElementField)
                {
                    var oldValueTable = $"Old{refCostElementField.ReferenceMeta.Name}";

                    selectColumns.Add(new ColumnInfo(refCostElementField.ReferenceFaceField, oldValueTable, OldValueName));
                    joinInfos.Add(new JoinInfo(costBlockMeta, costBlockMeta.CostElementsApprovedFields[costElementField].Name, oldValueTable));
                }
                else
                {
                    selectColumns.Add(new ColumnInfo(costBlockMeta.CostElementsApprovedFields[costElementField].Name, costBlockMeta.Name, OldValueName));
                }

                selectColumns.Add(new ColumnInfo(MetaConstants.IdFieldKey, costBlockMeta.HistoryMeta.Name, "HistoryValueId"));
                selectColumns.AddRange(GetCoordinateColumns());

                joinInfos.AddRange(GetCoordinateJoinInfos());

                var selectQuery = this.historyQueryBuilder.BuildSelectHistoryValueQuery(history.Context, selectColumns);

                return this.historyQueryBuilder.BuildJoinApproveHistoryValueQuery(history, selectQuery, inputLevelJoinType, joinInfos, historyValueId, costBlockFilter);

                IEnumerable<ColumnInfo> GetCoordinateColumns()
                {
                    foreach (var dependecyField in domainCoordinateFields)
                    {
                        yield return new ColumnInfo(dependecyField.Name, costBlockMeta.Name);
                        yield return new ColumnInfo(dependecyField.ReferenceFaceField, GetAlias(dependecyField.ReferenceMeta), $"{dependecyField.Name}_Name");
                    }
                }

                IEnumerable<JoinInfo> GetCoordinateJoinInfos()
                {
                    return domainCoordinateFields.Select(field => new JoinInfo
                    {
                        Meta = costBlockMeta,
                        ReferenceFieldName = field.Name,
                        JoinedTableAlias = GetAlias(field.ReferenceMeta)
                    });
                }

                string GetAlias(BaseEntityMeta meta) => $"{meta.Schema}_{meta.Name}";
            }
        }

        public async Task<int> Approve(CostBlockHistory history)
        {
            var costBlockMeta = this.domainEnitiesMeta.CostBlocks[history.Context];
            var costElementField = costBlockMeta.CostElementsFields[history.Context.CostElementId];
            var historyValueColumn = new ColumnSqlBuilder
            {
                Table = costBlockMeta.HistoryMeta.Name,
                Name = costElementField.Name
            };

            var costElementColumn = new QueryUpdateColumnInfo(costElementField.Name, historyValueColumn, costBlockMeta.Name);
            var costElementApprovedField = costBlockMeta.CostElementsApprovedFields[costElementField];
            var costElementApprovedColumn = new QueryUpdateColumnInfo(costElementApprovedField.Name, historyValueColumn, costBlockMeta.Name);

            var updateQuery =
                Sql.Update(costBlockMeta, costElementColumn, costElementApprovedColumn)
                   .From(costBlockMeta.HistoryMeta);

            var query = this.historyQueryBuilder.BuildJoinApproveHistoryValueQuery(history, updateQuery);

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        public async Task<IEnumerable<BundleDetail>> GetApproveBundleDetailQualityGate(
            CostBlockHistory history, 
            bool userCountyGroupCheck, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.CostBlocks[history.Context];
            var query = this.qualityGateQueryBuilder.BuildQulityGateApprovalQuery(history, userCountyGroupCheck, historyValueId, costBlockFilter);

            var maxInputLevelId = this.GetMaxInputLevelId(history, historyValueId);

            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId, maxInputLevelId)
            {
                OldValue = true,
                UsePeriodQualityGate = true,
                UseHistoryValueId = true,
                UsetCountryGroupQualityGate = userCountyGroupCheck
            };

            return await this.repositorySet.ReadBySqlAsync(query, mapper.Map);
        }

        private string GetMaxInputLevelId(CostBlockHistory history, long? historyValueId)
        {
            return historyValueId.HasValue ? null : history.Context.InputLevelId;
        }
    }
}
