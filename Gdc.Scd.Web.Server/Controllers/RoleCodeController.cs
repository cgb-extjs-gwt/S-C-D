using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class RoleCodeController : BaseDomainController<RoleCode>
    {
        public RoleCodeController(IDomainService<RoleCode> domainService):base(domainService)
        {         
        }
    }
}