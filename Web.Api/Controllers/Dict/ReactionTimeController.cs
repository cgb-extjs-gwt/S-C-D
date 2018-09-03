using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class ReactionTimeController : BaseDomainController<ReactionTime>
    {
        public ReactionTimeController(IDomainService<ReactionTime> domainService) : base(domainService) { }
    }
}
