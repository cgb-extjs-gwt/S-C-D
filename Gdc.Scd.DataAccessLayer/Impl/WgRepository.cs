using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class WgRepository : EntityFrameworkRepository<Wg>, IWgRepository
    {
        public WgRepository(EntityFrameworkRepositorySet repositorySet) : base(repositorySet) { }

        public IQueryable<Wg> GetStandards()
        {
            return GetAll().Include(wg => wg.Sog)
                           .FromSql("SELECT * FROM InputAtoms.WgStdView")
                           .Where(x => x.WgType == WgType.Por);
        }
    }
}
