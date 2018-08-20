using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class DurationController : BaseDomainController<Duration>
    {
        public DurationController(IDomainService<Duration> domainService) : base(domainService) { }
    }
}
