using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class ServiceLocationController : BaseDomainController<ServiceLocation>
    {
        public ServiceLocationController(IDomainService<ServiceLocation> domainService) : base(domainService) { }
    }
}
