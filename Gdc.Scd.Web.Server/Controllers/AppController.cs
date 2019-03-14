using System.Net.Http;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class AppController : ApiController
    {
        private readonly IAppService appService;

        public AppController(IAppService appService)
        {
            this.appService = appService;
        }

        [HttpGet]
        public AppData GetAppData()
        {
            var appData = this.appService.GetAppData();
            if (appData.IsAuthorized)
                return appData;
            var message = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "You are not authorized." };
            throw new HttpResponseException(message);
        }
    }
}