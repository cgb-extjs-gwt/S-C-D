using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioPivotGridService : IPortfolioPivotGridService
    {
        private readonly DomainEnitiesMeta meta;

        private readonly IPivotGridRepository pivotGridRepository;

        private readonly IPortfolioPivotGridQueryBuilder portfolioPivotGridQueryBuilder;

        public PortfolioPivotGridService(
            DomainEnitiesMeta meta, 
            IPivotGridRepository pivotGridRepository, 
            IPortfolioPivotGridQueryBuilder portfolioPivotGridQueryBuilder)
        {
            this.meta = meta;
            this.pivotGridRepository = pivotGridRepository;
            this.portfolioPivotGridQueryBuilder = portfolioPivotGridQueryBuilder;
        }

        public async Task<PivotResult> GetData(PivotRequest request)
        {
            var queryMeta = this.portfolioPivotGridQueryBuilder.Build(request);

            return await this.pivotGridRepository.GetData(request, queryMeta.Meta, queryMeta.Query);
        }
    }
}
