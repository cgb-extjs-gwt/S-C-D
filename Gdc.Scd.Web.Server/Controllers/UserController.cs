using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class UserController : BaseDomainController<User>
    {
        public UserController(IDomainService<User> domainService) : base(domainService) { }
    }
}