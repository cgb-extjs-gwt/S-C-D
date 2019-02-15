using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class WgController : BaseDomainController<Wg>
    {
        IWgService wgService;
        IWgPorService wgPorService;

        public WgController(
                IWgService wgService,
                IWgPorService wgPorService
            ) : base(wgPorService)
        {
            this.wgService = wgService;
            this.wgPorService = wgPorService;
        }

        [HttpGet]
        public Task<Wg[]> Multivendor()
        {
            return wgService.GetAll()
                            .Where(x => !x.DeactivatedDateTime.HasValue)
                            .GetAsync();
        }

        [HttpGet]
        public Task<Wg[]> Standard()
        {
            return wgService.GetStandards().GetAsync();
        }

    }
}
