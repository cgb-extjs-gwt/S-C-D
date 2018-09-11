using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class HelperController : ApiController
    {
        public HttpResponseMessage GetApiUrl()
        {
            string responseBody =  Request.GetRequestContext().VirtualPathRoot + "/api/";
            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StringContent(responseBody, Encoding.UTF8, "text/plain");
            response.Content.Headers.LastModified = DateTime.Now;
            response.Headers.CacheControl = new CacheControlHeaderValue() { Public = true };

            return response;
        }
    }
}
