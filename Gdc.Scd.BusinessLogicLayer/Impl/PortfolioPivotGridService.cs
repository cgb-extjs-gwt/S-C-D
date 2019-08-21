using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioPivotGridService : IPortfolioPivotGridService
    {
        private readonly IPivotGridRepository portfolioPivotGridRepository;

        private readonly DomainEnitiesMeta meta;

        public PortfolioPivotGridService(IPivotGridRepository portfolioPivotGridRepository, DomainEnitiesMeta meta)
        {
            this.portfolioPivotGridRepository = portfolioPivotGridRepository;
            this.meta = meta;
        }

        public async Task<PivotResult> GetData(PivotRequest request)
        {
            return await this.portfolioPivotGridRepository.GetData(request, this.meta.LocalPortfolio);
        }
    }
}
