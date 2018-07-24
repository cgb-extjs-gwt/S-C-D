using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class ServiceLocationController : BaseDomainController<ServiceLocation>
    {
        public ServiceLocationController(IDomainService<ServiceLocation> domainService) : base(domainService) { }
    }
}
