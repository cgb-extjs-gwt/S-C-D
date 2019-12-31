using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class UserRepository : EntityFrameworkRepository<User>, IUserRepository
    {
        public UserRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public IQueryable<User> GetAllWithRoles()
        {
            return
                this.GetAll()
                    .Include(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                    .ThenInclude(rolePermission => rolePermission.Permission)
                    .Include(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.User)
                    .Include(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Country);
        }

        public override void Save(User item)
        {
            if (item.UserRoles != null)
            {
                var roleRepository = this.repositorySet.GetRepository<Role>();

                foreach (var userRole in item.UserRoles)
                {
                    this.SetAddOrUpdateState(userRole);
                    this.SetAddOrUpdateState(userRole.Country);
                    this.SetAddOrUpdateState(userRole.User);

                    roleRepository.Save(userRole.Role);
                }
            }

            base.Save(item);
        }
    }
}
