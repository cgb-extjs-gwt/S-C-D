using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Gdc.Scd.Core.Entities
{
    public class Role : NamedId
    {
        public bool IsGlobal { get; set; }

        [JsonIgnore]
        public List<RolePermission> RolePermissions { get; set; }

        [NotMapped]
        [JsonIgnore]
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
