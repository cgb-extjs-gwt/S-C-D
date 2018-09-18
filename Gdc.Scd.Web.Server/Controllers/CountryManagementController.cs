using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CountryManagementController : BaseDomainController<Country>
    {
        public CountryManagementController(IDomainService<Country> countryService) : base(countryService) { }
    }
}