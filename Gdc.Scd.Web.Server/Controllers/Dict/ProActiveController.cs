using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class ProActiveController : ReadonlyDomainController<ProActiveSla>
    {
        public ProActiveController(IDomainService<ProActiveSla> domainService) : base(domainService) { }

        public override Task<ProActiveSla[]> GetAll()
        {
            return this.ctrl.GetAll().ContinueWith(x => x.Result.Select(y => new ProActiveSla
            {
                Id = y.Id,
                Name = y.ExternalName,
                ExternalName = y.ExternalName
            }).ToArray());
        }
    }
}
