using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class SortableDomainService<T> : DomainService<T>, 
                                                    ISortableDomainService<T> where T : class, IIdentifiable, ISortable, new()
    {
        public SortableDomainService(IRepositorySet repositorySet)
            : base(repositorySet)
        {
        }

        public override IQueryable<T> GetAll()
        {
            return base.GetAll().OrderBy(entity => entity.Order);
        }
    }
}
