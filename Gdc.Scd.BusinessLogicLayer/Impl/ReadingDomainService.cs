using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ReadingDomainService<T> : IReadingDomainService<T> where T : class, IIdentifiable, new()
    {
        protected readonly IRepositorySet repositorySet;

        protected readonly IRepository<T> repository;

        public ReadingDomainService(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
            this.repository = this.repositorySet.GetRepository<T>();
        }

        public virtual T Get(long id)
        {
            return this.repository.Get(id);
        }

        public virtual IQueryable<T> GetAll()
        {
            return this.repository.GetAll();
        }
        public virtual IQueryable<T> GetAllAsTracking()
        {
            return this.repository.GetAllAsTracking();
        }
    }
}
