using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class UserRoleController : BaseDomainController<UserRole>
    {
        public UserRoleController(IDomainService<UserRole> domainService) : base(domainService) { }
    }
}