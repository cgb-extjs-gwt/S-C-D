using Gdc.Scd.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return base.GetAll().Include(c => c.CountryGroup);
        }
    }
}
