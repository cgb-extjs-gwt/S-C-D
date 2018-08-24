using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class WarrantyGroupController : BaseDomainController<Wg>
    {
        public WarrantyGroupController(IDomainService<Wg> domainService) : base(domainService)
        {
        }
    }
}