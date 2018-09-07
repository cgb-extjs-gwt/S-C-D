using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class RoleCodeController : BaseDomainController<RoleCode>
    {
        public RoleCodeController(IDomainService<RoleCode> domainService):base(domainService)
        {         
        }
    }
}