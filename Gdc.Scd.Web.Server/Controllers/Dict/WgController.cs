using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class WgController : BaseDomainController<Wg>
    {
        public WgController(IWgPorService domainService) : base(domainService) { }
    }
}
