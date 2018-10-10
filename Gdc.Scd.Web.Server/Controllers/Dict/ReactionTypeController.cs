using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class ReactionTypeController : BaseDomainController<ReactionType>
    {
        public ReactionTypeController(IDomainService<ReactionType> domainService) : base(domainService) { }
    }
}
