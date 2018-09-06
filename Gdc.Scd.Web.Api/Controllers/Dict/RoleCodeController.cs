using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class RoleCodeController : BaseDomainController<RoleCode>
    {
        public RoleCodeController(IDomainService<RoleCode> domainService) 
            : base(domainService) { }
    }
}