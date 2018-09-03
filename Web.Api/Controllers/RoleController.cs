using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class RoleController : BaseDomainController<Role>
    {
        public RoleController(IDomainService<Role> domainService) : base(domainService)
        {
        }
    }
}