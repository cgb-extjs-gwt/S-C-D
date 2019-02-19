using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class CurrencyController : ReadonlyDomainController<Currency>
    {
        public CurrencyController(IDomainService<Currency> domainService) : base(domainService) { }
    }
}
