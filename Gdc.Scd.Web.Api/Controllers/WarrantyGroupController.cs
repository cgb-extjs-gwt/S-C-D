using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class WarrantyGroupController : BaseDomainController<Wg>
    {
        public WarrantyGroupController(IDomainService<Wg> domainService) : base(domainService)
        {
        }

        public override IEnumerable<Wg> GetAll()
        {
            var a = this.domainService.GetAll().ToList(); ;

            return this.domainService.GetAll().Select(t => new Wg
            {
                Id = t.Id,
                Name = t.Name,
                RoleCode = t.RoleCode,
                RoleCodeId = t.RoleCode == null ? 0 : t.RoleCode.Id
            }).ToList();
        }
    }
}