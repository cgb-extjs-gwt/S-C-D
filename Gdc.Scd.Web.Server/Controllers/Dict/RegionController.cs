using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class RegionController : ReadonlyDomainController<Region>
    {
        public RegionController(IDomainService<Region> domainService) : base(domainService) { }
    }
}
