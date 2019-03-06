using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    /// <summary>
    /// Readonly wrap over BaseDomainController<T>
    /// </summary>
    public abstract class ReadonlyDomainController<T> : ApiController where T : IIdentifiable
    {
        protected BaseDomainController<T> ctrl;

        protected readonly IDomainService<T> domainService;

        public ReadonlyDomainController(IDomainService<T> domainService)
        {
            this.domainService = domainService;
            this.ctrl = new DomainController(domainService);
        }

        public virtual Task<T[]> GetAll()
        {
            return this.ctrl.GetAll();
        }

        public virtual Task<DataInfo<T>> GetBy(int start, int limit, string sort = null, string filter = null)
        {
            return this.ctrl.GetBy(start, limit, sort, filter);
        }

        public virtual Task<T> Get(long id)
        {
            return this.ctrl.Get(id);
        }

        private class DomainController : BaseDomainController<T>
        {
            public DomainController(IDomainService<T> domainService) : base(domainService) { }
        }
    }
}