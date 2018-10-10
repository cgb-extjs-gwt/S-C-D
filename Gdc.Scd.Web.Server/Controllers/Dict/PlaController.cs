using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class PlaController : BaseDomainController<Pla>
    {
        public PlaController(IDomainService<Pla> domainService) : base(domainService) { }
    }
}
