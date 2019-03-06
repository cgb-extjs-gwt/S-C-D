using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class SwDigitController : ReadonlyDomainController<SwDigit>
    {
        private readonly ISwDigitService digitService;

        public SwDigitController(ISwDigitService domainService) : base(domainService)
        {
            this.digitService = domainService;
        }

        [HttpGet]
        public Task<Sog[]> Sog()
        {
            return digitService.GetDigitSog().GetAsync();
        }
    }
}