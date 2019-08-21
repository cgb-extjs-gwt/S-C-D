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

        private readonly IPivotGridRepository portfolioPivotGridRepository;

        private readonly IPortfolioPivotGridQueryBuilder portfolioPivotGridQueryBuilder;

        public PortfolioPivotGridService(
            DomainEnitiesMeta meta, 
            IPivotGridRepository portfolioPivotGridRepository, 
            IPortfolioPivotGridQueryBuilder portfolioPivotGridQueryBuilder)
        {
            this.meta = meta;
            this.portfolioPivotGridRepository = portfolioPivotGridRepository;
            this.portfolioPivotGridQueryBuilder = portfolioPivotGridQueryBuilder;
        }

        public async Task<PivotResult> GetData(PivotRequest request)
        {
            var queryMeta = this.portfolioPivotGridQueryBuilder.Build();

            return await this.portfolioPivotGridRepository.GetData(request, queryMeta.Meta, queryMeta.Query);
        }
    }
}
