using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class ProActiveController : ReadonlyDomainController<ProActiveSla>
    {
        public ProActiveController(IDomainService<ProActiveSla> domainService) : base(domainService) { }
    }
}
