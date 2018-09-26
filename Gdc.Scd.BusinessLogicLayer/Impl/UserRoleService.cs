using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class UserRoleService : DomainService<UserRole>, IUserRoleService
    {
        public UserRoleService(IRepositorySet repositorySet)
            : base(repositorySet)
        {
        }

        public List<Role> GetUserRoles(User user, Country country)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Role> GetCurrentUserRoles()
        {
            throw new System.NotImplementedException();
        }

        public bool IsUserInRole(User user, Role role, Country country=null)
        {
            var roles = this.GetAll().Where(x => x.UserId == user.Id).Select(x => x.Role);

            return this.GetAll().Where(x => x.UserId == user.Id).Select(x => x.RoleId).Contains(role.Id);
        }

    }
}
