using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class UserController : BaseDomainController<User>
    {
        public UserController(IDomainService<User> domainService) : base(domainService)
        {
        }
    }
}