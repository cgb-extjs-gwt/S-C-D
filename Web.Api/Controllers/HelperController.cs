using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Web.Api.Controllers
{
    public class HelperController : ApiController
    {
        public HttpResponseMessage GetObservablesCode()
        {
            string responseBody = "var API_URL=\"" + Request.GetRequestContext().VirtualPathRoot + "/api/\"";
            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StringContent(responseBody, Encoding.UTF8, "text/plain");
            response.Content.Headers.LastModified = DateTime.Now;
            response.Headers.CacheControl = new CacheControlHeaderValue() { Public = true };
           
            return response;
        }
    }
}