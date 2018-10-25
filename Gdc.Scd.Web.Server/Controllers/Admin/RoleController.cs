using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Admin })]
    public class RoleController : BaseDomainController<Role>
    {
        public RoleController(IDomainService<Role> domainService) : base(domainService) { }
    }
}