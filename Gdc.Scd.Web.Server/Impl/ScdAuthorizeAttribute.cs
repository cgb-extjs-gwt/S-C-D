using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.Web.Server.Impl
{
    public class ScdAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        public string[] Permissions { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var result = false;
            var principalIdintity = actionContext.RequestContext.Principal.Identity;

            if (principalIdintity.IsAuthenticated)
            {
                var userService = (IUserService)DependencyResolver.Current.GetService(typeof(IUserService));

                result =
                    this.IsPermissionsAuthorized(principalIdintity.Name, userService) &&
                    this.IsRolesAuthorized(principalIdintity.Name, userService) &&
                    this.IsUsersAuthorized(principalIdintity.Name, userService);
            }

            return result;
        }

        private bool IsUsersAuthorized(string userLogin, IUserService userService)
        {
            return string.IsNullOrEmpty(this.Users) || this.Split(this.Users).Contains(userLogin);
        }

        private bool IsRolesAuthorized(string userLogin, IUserService userService)
        {
            return string.IsNullOrEmpty(this.Roles) || userService.HasRole(userLogin, this.Split(this.Roles));
        }

        private bool IsPermissionsAuthorized(string userLogin, IUserService userService)
        {
            return this.Permissions == null|| userService.HasPermission(userLogin, this.Permissions);
        }

        private string[] Split(string value)
        {
            return value.Split(',');
        }
    }
}