using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Gdc.Scd.Core.Entities
{
    public class Role : NamedId
    {
        public bool IsGlobal { get; set; }

        public List<RolePermission> RolePermissions { get; set; }

        [NotMapped]
        public IEnumerable<Permission> Permissions
        {
            get
            {
                IEnumerable<Permission> permissions = null;

                if (this.RolePermissions != null)
                {
                    permissions = this.RolePermissions.Select(rolePermission => rolePermission.Permission);
                }

                return permissions;
            }
        }
    }
}
