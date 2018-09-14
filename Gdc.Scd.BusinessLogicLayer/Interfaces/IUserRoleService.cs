using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    interface IUserRoleService
    {
        bool IsUserInRole(User user, Role role, Country country);
        List<Role> GetUserRoles(User user, Country country);
    }
}
