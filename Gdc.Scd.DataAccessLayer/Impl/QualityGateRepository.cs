using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
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

        public async Task<IEnumerable<CostBlockValueHistory>> Check(
            HistoryContext historyContext,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var query = this.qualityGateQueryBuilder.BuildQualityGateQuery(historyContext, editItems, costBlockFilter);
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta)
            {
                UseQualityGate = true,
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetailQualityGate(CostBlockHistory history, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var query = this.qualityGateQueryBuilder.BuildQulityGateApprovalQuery(history, historyValueId, costBlockFilter);
            var maxInputLevelId = historyValueId.HasValue ? null : history.Context.InputLevelId;
            var mapper = new CostBlockValueHistoryMapper(costBlockMeta, maxInputLevelId)
            {
                UseQualityGate = true,
                UseHistoryValueId = true
            };

            return await this.repositorySet.ReadBySql(query, mapper.Map);
        }
    }
}
