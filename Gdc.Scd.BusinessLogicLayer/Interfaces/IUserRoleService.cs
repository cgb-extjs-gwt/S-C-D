using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IUserRoleService 
    {
        bool IsUserInRole(User user, Role role, Country country);

        List<Role> GetUserRoles(User user, Country country);

        IEnumerable<Role> GetCurrentUserRoles();
    }
}
