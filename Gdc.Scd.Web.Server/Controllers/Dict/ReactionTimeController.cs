using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class ReactionTimeController : BaseDomainController<ReactionTime>
    {
        public ReactionTimeController(IDomainService<ReactionTime> domainService) : base(domainService) { }
    }
}
