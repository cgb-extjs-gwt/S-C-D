using Gdc.Scd.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class SwDigitRepository : EntityFrameworkRepository<SwDigit>
    {
        public SwDigitRepository(EntityFrameworkRepositorySet repositorySet)
            : base(repositorySet)
        {
        }

        public override IQueryable<SwDigit> GetAll()
        {
            return
                base.GetAll()
                    .Include(digit => digit.Sog);
        }
    }
}
