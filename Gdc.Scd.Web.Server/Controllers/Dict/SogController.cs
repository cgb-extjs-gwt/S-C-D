using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class SogController : ReadonlyDomainController<Sog>
    {
        public SogController(IDomainService<Sog> domainService) : base(domainService) { }
    }
}