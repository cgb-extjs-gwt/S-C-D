using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class RoleController : BaseDomainController<Role>
    {
        public RoleController(IDomainService<Role> domainService) : base(domainService) { }
    }
}