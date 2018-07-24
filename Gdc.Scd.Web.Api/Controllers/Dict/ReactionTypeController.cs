using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class ReactionTypeController : BaseDomainController<ReactionType>
    {
        public ReactionTypeController(IDomainService<ReactionType> domainService) : base(domainService) { }
    }
}
