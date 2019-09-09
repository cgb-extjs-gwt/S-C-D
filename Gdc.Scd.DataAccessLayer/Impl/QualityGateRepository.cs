using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;
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

        public async Task<IEnumerable<BundleDetail>> Check(CostElementContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, long[]> costBlockFilter, bool userCountyGroupCheck)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var query = this.qualityGateQueryBuilder.BuildQualityGateQuery(historyContext, editItems, costBlockFilter.Convert(), userCountyGroupCheck);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, historyContext.CostElementId)
            {
                OldValue = true,
                UsePeriodQualityGate = true,
                UsetCountryGroupQualityGate = userCountyGroupCheck
            };

            return await this.repositorySet.ReadBySqlAsync(query, mapper.Map);
        }

        public async Task<IEnumerable<BundleDetail>> Check(CostBlockHistory history, bool userCountyGroupCheck, IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var query = this.qualityGateQueryBuilder.BuildQualityGateQuery(history, userCountyGroupCheck, costBlockFilter);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, history.Context.CostElementId)
            {
                OldValue = true,
                UsePeriodQualityGate = true,
                UsetCountryGroupQualityGate = userCountyGroupCheck
            };

            return await this.repositorySet.ReadBySqlAsync(query, mapper.Map);
        }
    }
}
