using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Gdc.Scd.Core.Entities
{
    public class User : NamedId
    {
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
                    permissions = roles.SelectMany(role => role.Permissions);
                }

                return permissions;
            }
        }
    }
}
