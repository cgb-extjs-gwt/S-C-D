using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gdc.Scd.Web.Server.Impl;
using Gdc.Scd.Core.Constants;
using System.Threading.Tasks;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Admin })]
    public class RoleCodeController : BaseDomainController<RoleCode>
    {
        private readonly IRoleCodeService roleCodeService;

        public RoleCodeController(IDomainService<RoleCode> domainService, IRoleCodeService roleCodeService) : base(domainService)
        {
            this.roleCodeService = roleCodeService;
        }

        [HttpPost]
        public HttpResponseMessage DeactivateAll([FromBody]IEnumerable<RoleCode> items)
        {
            try
            {
                roleCodeService.Deactivate(items);
            }
            catch(Exception ex)
            {
                HttpError err = new HttpError(ex.Message);
                return Request.CreateResponse(HttpStatusCode.Conflict, err);
            }
                
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        public Task<RoleCode[]> GetAllActive()
        {
            return roleCodeService.GetAllActive();
        }

        [HttpPost]
        public override void SaveAll([FromBody]IEnumerable<RoleCode> items)
        {
            this.roleCodeService.Save(items);
        }
    }
}