using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
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

        public Task AllowCombination()
        {
            return Task.FromResult(0);
        }

        public Task DenyCombination()
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<CapabilityMatrixAllow>> GetAllowedCombinations()
        {
            return AllowRepo().GetAllAsync();
        }

        public Task<IEnumerable<CapabilityMatrixDeny>> GetDenyedCombinations()
        {
            return DenyRepo().GetAllAsync();
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
