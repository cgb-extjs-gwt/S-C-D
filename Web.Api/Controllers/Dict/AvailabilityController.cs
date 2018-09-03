using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class AvailabilityController : BaseDomainController<Availability>
    {
        public AvailabilityController(IDomainService<Availability> domainService) : base(domainService) { }
    }
}
