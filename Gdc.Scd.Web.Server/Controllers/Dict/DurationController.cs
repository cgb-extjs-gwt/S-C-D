using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class DurationController : ReadonlyDomainController<Duration>
    {
        public DurationController(IDomainService<Duration> domainService) : base(domainService) { }
    }
}
