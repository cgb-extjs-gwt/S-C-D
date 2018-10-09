using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using System.Web.Http;
using System.Net.Http;
using System.Data.SqlClient;
using System.Net;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class RoleCodeController : BaseDomainController<RoleCode>
    {
        public RoleCodeController(IDomainService<RoleCode> domainService):base(domainService)
        {         
        }

        [HttpPost]
        public override HttpResponseMessage DeleteAll([FromBody]IEnumerable<RoleCode> items)
        {
            try
            {
                foreach (var item in items)
                {
                    this.domainService.Delete(item.Id);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException &&
                        ((SqlException)ex.InnerException).Number == 547)
                {
                    HttpError err = new HttpError("This item cannot be deleted because it is still referenced by other items.");
                    return Request.CreateResponse(HttpStatusCode.Conflict, err);
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}