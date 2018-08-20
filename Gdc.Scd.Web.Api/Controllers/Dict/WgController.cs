using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class WgController : BaseDomainController<Wg>
    {
        public WgController(IDomainService<Wg> domainService) : base(domainService) { }
    }
}
