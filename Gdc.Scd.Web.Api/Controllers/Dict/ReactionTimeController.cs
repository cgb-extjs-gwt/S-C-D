using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class ReactionTimeController : BaseDomainController<ReactionTime>
    {
        public ReactionTimeController(IDomainService<ReactionTime> domainService) : base(domainService) { }
    }
}
