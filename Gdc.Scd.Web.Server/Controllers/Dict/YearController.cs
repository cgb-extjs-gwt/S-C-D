using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class YearController : ReadonlyDomainController<Year>
    {
        public YearController(IDomainService<Year> domainService) : base(domainService) { }
    }
}