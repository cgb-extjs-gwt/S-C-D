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
            return this.appService.GetAppData();
        }
    }
}