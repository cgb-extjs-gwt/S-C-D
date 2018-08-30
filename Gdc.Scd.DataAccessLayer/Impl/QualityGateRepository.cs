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

        private readonly ICostBlockValueHistoryMapper costBlockValueHistoryMapper;

        public QualityGateRepository(
            IQualityGateQueryBuilder qualityGateQueryBuilder, 
            IRepositorySet repositorySet,
            ICostBlockValueHistoryMapper costBlockValueHistoryMapper,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.qualityGateQueryBuilder = qualityGateQueryBuilder;
            this.repositorySet = repositorySet;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.costBlockValueHistoryMapper = costBlockValueHistoryMapper;
        }

        public async Task<IEnumerable<CostBlockValueHistory>> Check(
            HistoryContext historyContext,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var query = this.qualityGateQueryBuilder.BuildQualityGateQuery(historyContext, editItems, costBlockFilter);

            return await this.repositorySet.ReadBySql(
                query, 
                reader => this.costBlockValueHistoryMapper.MapWithQualityGate(costBlockMeta, reader));
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetailQualityGate(CostBlockHistory history, long historyValueId)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var query = this.qualityGateQueryBuilder.BuildQulityGateHistoryQuery(history, historyValueId);

            return await this.repositorySet.ReadBySql(
                query,
                reader => this.costBlockValueHistoryMapper.MapWithQualityGate(costBlockMeta, reader));
        }
    }
}
