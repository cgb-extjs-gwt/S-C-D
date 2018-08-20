using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class DurationController : BaseDomainController<Duration>
    {
        public DurationController(IDomainService<Duration> domainService) : base(domainService) { }
    }
}
