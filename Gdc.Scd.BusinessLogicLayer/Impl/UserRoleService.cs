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

        public List<Role> GetUserRoles(User user, Country country = null)
        {
            var userRoles = this.GetAll().Where(x => x.UserId == user.Id);
            if (country == null)
            {
                return userRoles.Where(x => x.Role.IsGlobal).Select(x=>x.Role).ToList();
            }
            else
            {
                return userRoles.Where(x => x.Role.IsGlobal || x.CountryId == country.Id).Select(x => x.Role).ToList();
            }
        }

        public bool IsUserInRole(User user, Role role, Country country=null)
        {
            var userRoles = this.GetAll().Where(x => x.UserId == user.Id && x.RoleId == role.Id);
            if (country == null)
            {
                return userRoles.Where(x => x.Role.IsGlobal).Any();
            }
            else
            {
                return userRoles.Where(x => x.Role.IsGlobal || x.CountryId==country.Id).Any();
            } 
        }

    }
}
