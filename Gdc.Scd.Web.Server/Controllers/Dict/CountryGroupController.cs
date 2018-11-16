using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class CountryGroupController : ReadonlyDomainController<CountryGroup>
    {
        public CountryGroupController(IDomainService<CountryGroup> domainService) : base(domainService) { }
    }
}
