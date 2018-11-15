using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class QualityGateRepository : IQualityGateRepository
    {
        private readonly IQualityGateQueryBuilder qualityGateQueryBuilder;

        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public QualityGateRepository(
            IQualityGateQueryBuilder qualityGateQueryBuilder, 
            IRepositorySet repositorySet,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.qualityGateQueryBuilder = qualityGateQueryBuilder;
            this.repositorySet = repositorySet;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<IEnumerable<BundleDetail>> Check(HistoryContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, long[]> costBlockFilter)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var query = this.qualityGateQueryBuilder.BuildQualityGateQuery(historyContext, editItems, costBlockFilter.Convert(), true);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, historyContext.CostElementId)
            {
                UsePeriodQualityGate = true,
                UsetCountryGroupQualityGate = true
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }

        public async Task<IEnumerable<BundleDetail>> Check(CostBlockHistory history, IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var query = this.qualityGateQueryBuilder.BuildQualityGateQuery(history, true, costBlockFilter);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId)
            {
                UsePeriodQualityGate = true,
                UsetCountryGroupQualityGate = true
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }

        public async Task<IEnumerable<BundleDetail>> GetApproveBundleDetailQualityGate(CostBlockHistory history, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var query = this.qualityGateQueryBuilder.BuildQulityGateApprovalQuery(history, true, historyValueId, costBlockFilter);

            var maxInputLevelId = historyValueId.HasValue ? null : history.Context.InputLevelId;

            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId, maxInputLevelId)
            {
                UsePeriodQualityGate = true,
                UseHistoryValueId = true,
                UsetCountryGroupQualityGate = true
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }
    }
}
