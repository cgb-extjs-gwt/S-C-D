using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CapabilityMatrixService : ICapabilityMatrixService
    {
        private readonly IRepositorySet repositorySet;

        private IRepository<CapabilityMatrixAllow> allowRepo;

        private IRepository<CapabilityMatrixDeny> denyRepo;

        public CapabilityMatrixService(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public Task AllowCombination(CapabilityMatrixEditDto m)
        {
            return Task.FromResult(0);
        }

        public void AllowCombinations(long[] items)
        {
            var repo = DenyRepo();

            for (var i = 0; i < items.Length; i++)
            {
                repo.Delete(items[i]);
            }

            repositorySet.Sync();
        }

        public Task DenyCombination(CapabilityMatrixEditDto m)
        {
            return Task.FromResult(0);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations()
        {
            return AllowRepo()
                .GetAll()
                .Where(x => !DenyRepo().GetAll().Any(y => y.Id == x.Id))
                .Select(x => new CapabilityMatrixDto
                {
                    Id = x.Id,

                    Country = x.Country == null ? null : x.Country.Name,
                    Wg = x.Wg == null ? null : x.Wg.Name,
                    Availability = x.Availability == null ? null : x.Availability.Name,
                    Duration = x.Duration == null ? null : x.Duration.Name,
                    ReactionType = x.ReactionType == null ? null : x.ReactionType.Name,
                    ReactionTime = x.ReactionTime == null ? null : x.ReactionTime.Name,
                    ServiceLocation = x.ServiceLocation == null ? null : x.ServiceLocation.Name,

                    IsGlobalPortfolio = x.FujitsuGlobalPortfolio,
                    IsMasterPortfolio = x.MasterPortfolio,
                    IsCorePortfolio = x.CorePortfolio
                })
                .ToList();
        }

        public IEnumerable<CapabilityMatrixDto> GetDeniedCombinations()
        {
            return DenyRepo()
                .GetAll()
                .Select(x => new CapabilityMatrixAllow {
                    Id = x.Id,

                    Country =x.CapabilityMatrixAllow.Country,
                    Wg = x.CapabilityMatrixAllow.Wg,
                    Availability = x.CapabilityMatrixAllow.Availability,
                    Duration = x.CapabilityMatrixAllow.Duration,
                    ReactionType=x.CapabilityMatrixAllow.ReactionType,
                    ReactionTime = x.CapabilityMatrixAllow.ReactionTime,
                    ServiceLocation = x.CapabilityMatrixAllow.ServiceLocation,

                    FujitsuGlobalPortfolio = x.CapabilityMatrixAllow.FujitsuGlobalPortfolio,
                    MasterPortfolio = x.CapabilityMatrixAllow.MasterPortfolio,
                    CorePortfolio =x.CapabilityMatrixAllow.CorePortfolio
                })
                .Select(x => new CapabilityMatrixDto
                {
                    Id = x.Id,

                    Country = x.Country == null ? null : x.Country.Name,
                    Wg = x.Wg == null ? null : x.Wg.Name,
                    Availability = x.Availability == null ? null : x.Availability.Name,
                    Duration = x.Duration == null ? null : x.Duration.Name,
                    ReactionType = x.ReactionType == null ? null : x.ReactionType.Name,
                    ReactionTime = x.ReactionTime == null ? null : x.ReactionTime.Name,
                    ServiceLocation = x.ServiceLocation == null ? null : x.ServiceLocation.Name,

                    IsGlobalPortfolio = x.FujitsuGlobalPortfolio,
                    IsMasterPortfolio = x.MasterPortfolio,
                    IsCorePortfolio = x.CorePortfolio
                })
                .ToList();
        }

        protected virtual IRepository<CapabilityMatrixAllow> AllowRepo()
        {
            if (allowRepo == null)
            {
                allowRepo = repositorySet.GetRepository<CapabilityMatrixAllow>();
            }
            return allowRepo;
        }

        protected virtual IRepository<CapabilityMatrixDeny> DenyRepo()
        {
            if (denyRepo == null)
            {
                denyRepo = repositorySet.GetRepository<CapabilityMatrixDeny>();
            }
            return denyRepo;
        }
    }
}
