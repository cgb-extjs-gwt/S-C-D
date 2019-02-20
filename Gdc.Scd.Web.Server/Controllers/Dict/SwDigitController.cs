using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class SwDigitController : ReadonlyDomainController<SwDigit>
    {
        public SwDigitController(IDomainService<SwDigit> domainService) : base(domainService) { }
    }
}