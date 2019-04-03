using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
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

        public IQueryable<Wg> GetHardware()
        {
            return origin.GetAll().Include(wg => wg.Sog).Where(wg => wg.IsSoftware == false && !wg.DeactivatedDateTime.HasValue);
        }
    }
}
