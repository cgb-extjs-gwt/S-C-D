using System.Linq;
using Gdc.Scd.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class RoleRepository : EntityFrameworkRepository<Role>
    {
        public RoleRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override IQueryable<Role> GetAll()
        {
            return
                base.GetAll()
                    .Include(role => role.RolePermissions)
                    .ThenInclude(rolePermission => rolePermission.Role)
                    .Include(role => role.RolePermissions)
                    .ThenInclude(rolePermission => rolePermission.Permission);
        }

        public override void Save(Role item)
        {
            if (item.RolePermissions != null)
            {
                foreach (var rolePermission in item.RolePermissions)
                {
                    this.SetAddOrUpdateState(rolePermission);
                    this.SetAddOrUpdateState(rolePermission.Role);
                    this.SetAddOrUpdateState(rolePermission.Permission);
                }
            }

            base.Save(item);
        }
    }
}
