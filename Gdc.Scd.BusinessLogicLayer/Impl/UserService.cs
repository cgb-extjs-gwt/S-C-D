using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class UserService : DomainService<User>, IUserService
    {
        private readonly IPrincipalProvider principalProvider;

        private readonly IUserRepository userRepository;

        public UserService(IRepositorySet repositorySet, IPrincipalProvider principalProvider, IUserRepository userRepository)
            : base(repositorySet)
        {
            this.principalProvider = principalProvider;
            this.userRepository = userRepository;
        }

        public User GetCurrentUser()
        {
            var principal = this.principalProvider.GetCurrenctPrincipal();

            return
                this.userRepository.GetAllWithRoles()
                                   .FirstOrDefault(user => user.Login == principal.Identity.Name);
        }

        public bool HasPermission(string userLogin, params string[] permissionNames)
        {
            return
                this.GetUserRoles(userLogin)
                    .SelectMany(role => role.RolePermissions)
                    .Select(rolePermission => rolePermission.Permission.Name)
                    .Any(permissionName => permissionNames.Contains(permissionName));
        }

        public bool HasRole(string userLogin, params string[] roleNames)
        {
            return
                this.GetUserRoles(userLogin)
                    .Select(role => role.Name)
                    .Any(roleName => roleNames.Contains(roleName));
        }

        public IQueryable<Country> GetCurrentUserCountries()
        {
            var principal = this.principalProvider.GetCurrenctPrincipal();

            var userHasGlobalRole = this.GetAll()
                    .Where(user => user.Login == principal.Identity.Name)
                    .SelectMany(user => user.UserRoles)
                    .Where(userRole => userRole.Role.IsGlobal).Any();

            if (userHasGlobalRole)
            {
                return Enumerable.Empty<Country>().AsQueryable();
            }

            return
                this.GetAll()
                    .Where(user => user.Login == principal.Identity.Name)
                    .SelectMany(user => user.UserRoles)
                    .Where(userRole => userRole.Country != null)
                    .Select(userRole => userRole.Country);
        }

        public IQueryable<User> GetCountryKeyUsers()
        {
            return
                this.GetUsersByRole(
                    this.userRepository.GetAllWithRoles(),
                    RoleConstants.CountryKeyUser);
        }

        public IQueryable<User> GetAdmins()
        {
            return
                this.GetAll()
                    .Where(
                        user => user.UserRoles.Any(
                            userRole => userRole.Role.RolePermissions.Any(
                                rolePerm => rolePerm.Permission.Name == PermissionConstants.Admin)));
        }

        public IQueryable<User> GetPrsPsmUsers()
        {
            return 
                this.GetUsersByRole(
                    this.GetAll(), 
                    RoleConstants.PrsPsm);
        }

        private IQueryable<Role> GetUserRoles(string userLogin)
        {
            return
                this.GetAll()
                    .Where(user => user.Login == userLogin)
                    .SelectMany(user => user.UserRoles)
                    .Select(userRole => userRole.Role);
        }

        private IQueryable<User> GetUsersByRole(IQueryable<User> query, string roleName)
        {
            return query.Where(user => user.UserRoles.Any(userRole => userRole.Role.Name == roleName));
        }
    }
}
