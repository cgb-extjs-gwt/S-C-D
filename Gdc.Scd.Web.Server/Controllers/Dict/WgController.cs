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
        IDomainService<Wg> wgService;
        IWgPorService wgPorService;

        public WgController(
                IDomainService<Wg> wgService,
                IWgPorService wgPorService
            ) : base(wgPorService)
        {
            this.wgService = wgService;
            this.wgPorService = wgPorService;
        }

        [HttpGet]
        public Task<NamedId[]> Multivendor()
        {
            return wgService.GetAll()
                            .Where(x => !x.DeactivatedDateTime.HasValue)
                            .Select(x => new NamedId { Id = x.Id, Name = x.Name })
                            .GetAsync();
        }

        [HttpGet]
        public Task<NamedId[]> Standard()
        {
            return wgPorService.GetStandards()
                               .Select(x => new NamedId { Id = x.Id, Name = x.Name })
                               .GetAsync();
        }
    }
}
