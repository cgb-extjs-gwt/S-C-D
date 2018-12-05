using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class ServiceLocationController : ReadonlyDomainController<ServiceLocation>
    {
        public ServiceLocationController(IDomainService<ServiceLocation> domainService) : base(domainService) { }
    }
}
