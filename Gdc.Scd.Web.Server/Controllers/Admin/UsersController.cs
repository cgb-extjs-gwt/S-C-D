using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Gdc.Scd.Web.Server.Entities;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    public class UsersController : ApiController
    {
        private readonly IActiveDirectoryService activeDirectoryService;

        public UsersController(IActiveDirectoryService activeDirectoryService)
        {
            this.activeDirectoryService = activeDirectoryService;
        }

        [HttpGet]
        public void SelectUser([FromBody]DirectoryEntry user)
        {
            //TODO: need to add behavior
        }

        [HttpGet]
        public DataInfo<UserInfo> SearchUser(string _dc, string searchString, int page = 1, int start = 0, int limit = 25)
        {
            activeDirectoryService.Configuration = new Scd.BusinessLogicLayer.Helpers.ActiveDirectoryConfig
            {
                ForestName = ConfigurationManager.AppSettings["AdForestName"],
                DefaultDomain = ConfigurationManager.AppSettings["DefaultDomain"],
                AdServiceAccount = ConfigurationManager.AppSettings["AdServiceAccount"],
                AdServicePassword = ConfigurationManager.AppSettings["AdServicePassword"],
            };
            var foundUsers = activeDirectoryService.SearchForUserByString(searchString).Select(
                user => new UserInfo
                {
                    Username = user.DisplayName,
                    UserSamAccount = user.SamAccountName,
                }).ToList();

            return new DataInfo<UserInfo> { Items = foundUsers, Total = foundUsers.Count() };
        }
    }
}
