using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Gdc.Scd.Core.Entities
{
    public class User : NamedId
    {
        private const string SCD_ADMIN = "SCD Admin";

        public string Login { get; set; }

        public string Email { get; set; }

        public List<UserRole> UserRoles { get; set; }

        [NotMapped]
        public IEnumerable<Role> Roles
        {
            get
            {
                IEnumerable<Role> roles = null;

                if (this.UserRoles != null)
                {
                    roles = this.UserRoles.Select(userRoles => userRoles.Role);
                }

                return roles;
            }
        }

        [NotMapped]
        public IEnumerable<Permission> Permissions
        {
            get
            {
                IEnumerable<Permission> permissions = null;

                var roles = this.Roles;
                if (roles != null)
                {
                    permissions = roles.SelectMany(role => role.Permissions).Distinct();
                }

                return permissions;
            }
        }

        [NotMapped]
        public bool IsGlobal
        {
            get
            {
                var roles = this.Roles;
                if (roles != null)
                {
                    return roles.Any(x => x.IsGlobal);
                }
                return false;
            }
        }

        [NotMapped]
        public bool IsAdmin
        {
            get
            {
                var roles = this.Roles;
                if (roles != null)
                {
                    return roles.Any(x => string.Compare(x.Name, SCD_ADMIN, true) == 0);
                }
                return false;
            }
        }

        public bool HasPermission(string permissionName)
        {
            return this.Permissions.Any(permission => permission.Name == permissionName);
        }
    }
}
