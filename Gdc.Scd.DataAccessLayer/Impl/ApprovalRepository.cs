using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
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
            var query = this.historyQueryBuilder.BuildSelectJoinApproveHistoryValueQuery(history, historyValueId, costBlockFilter);
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId)
            {
                UseHistoryValueId = true
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }

        public async Task<int> Approve(CostBlockHistory history)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
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
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var query = this.qualityGateQueryBuilder.BuildQulityGateApprovalQuery(history, userCountyGroupCheck, historyValueId, costBlockFilter);

            var maxInputLevelId = historyValueId.HasValue ? null : history.Context.InputLevelId;

            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId, maxInputLevelId)
            {
                UsePeriodQualityGate = true,
                UseHistoryValueId = true,
                UsetCountryGroupQualityGate = userCountyGroupCheck
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }
    }
}
