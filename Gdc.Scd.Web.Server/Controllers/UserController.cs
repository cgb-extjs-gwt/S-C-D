using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Gdc.Scd.Web.Server.Entities;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class UsersController : Controller
    {
        private readonly IActiveDirectoryService activeDirectoryService;
        public UsersController(IActiveDirectoryService activeDirectoryService)
        {
            this.activeDirectoryService = activeDirectoryService;
        }
        public void SelectUser([System.Web.Http.FromBody]DirectoryEntry user)
        {
            //TODO: need to add behavior
        }
        public DataInfo<UserInfo> SearchUser(string _dc, string searchString, int page = 1, int start = 0, int limit = 25)
        {
            var foundUsers = activeDirectoryService.SearchForUserByString(searchString).Select(
                user => new UserInfo
                {
                    Username = user.Username,
                    UserSamAccount = activeDirectoryService.FindByIdentity(user.Username).SamAccountName,
                });

            return new DataInfo<UserInfo> { Items = foundUsers, Total = foundUsers.Count() };
        }
    }
}
