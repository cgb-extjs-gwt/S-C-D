using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class WgController : BaseDomainController<Wg>
    {
        public WgController(IDomainService<Wg> domainService) : base(domainService) { }
    }
}
