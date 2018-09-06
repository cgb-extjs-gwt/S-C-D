using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class YearController : BaseDomainController<Year>
    {
        public YearController(IDomainService<Year> domainService)
            : base(domainService) { }
    }
}
