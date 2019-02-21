using Gdc.Scd.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CountryRepository : EntityFrameworkRepository<Country>
    {
        public CountryRepository(EntityFrameworkRepositorySet repositorySet)
            : base(repositorySet)
        {
        }

        public override IQueryable<Country> GetAll()
        {
            return base.GetAll().Include(c => c.CountryGroup).Include(c => c.Region);
        }
    }
}
