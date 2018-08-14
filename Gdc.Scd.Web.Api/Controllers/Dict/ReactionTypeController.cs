using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers.Dict
{
    public class ReactionTypeController : BaseDomainController<ReactionType>
    {
        public ReactionTypeController(IDomainService<ReactionType> domainService) : base(domainService) { }
    }
}
