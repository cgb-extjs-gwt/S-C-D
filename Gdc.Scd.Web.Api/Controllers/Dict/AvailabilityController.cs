using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class AvailabilityController : BaseDomainController<Availability>
    {
        public AvailabilityController(IDomainService<Availability> domainService) : base(domainService) { }
    }
}
