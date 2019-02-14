using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class WgService : DomainService<Wg>, IWgService
    {
        private readonly IWgRepository origin;

        public WgService(IRepositorySet repositorySet, IWgRepository wgRepo) : base(repositorySet)
        {
            this.origin = wgRepo;
        }

        public IQueryable<Wg> GetStandards()
        {
            return origin.GetStandards();
        }
    }
}
