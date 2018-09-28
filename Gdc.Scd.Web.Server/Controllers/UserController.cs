using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Api.Entities;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class UsersController : ApiController
    {
        private readonly IActiveDirectoryService activeDirectoryService;
        public UsersController(IActiveDirectoryService activeDirectoryService)
        {
            this.activeDirectoryService = activeDirectoryService;
            activeDirectoryService.Configuration = new Scd.BusinessLogicLayer.Helpers.ActiveDirectoryConfig
            {
                ForestName = ConfigurationManager.AppSettings["AdForestName"],
                DefaultDomain = ConfigurationManager.AppSettings["DefaultDomain"],
                AdServiceAccount = ConfigurationManager.AppSettings["AdServiceAccount"],
                AdServicePassword = ConfigurationManager.AppSettings["AdServicePassword"],
            };
        }
        [System.Web.Http.HttpGet]
        public void SelectUser(string userIdentity, string _dc)
        {
            var userDirectoryEntry = activeDirectoryService.FindByIdentity(userIdentity);
            // some other behavior
        }
        [System.Web.Http.HttpGet]
        public DataInfo<UserInfo> SearchUser(string _dc, string searchString, int page = 1, int start = 0, int limit = 25)
        {
            var searchCount = Int32.Parse(ConfigurationManager.AppSettings["UsersSearchCount"]);
            if (string.IsNullOrEmpty(searchString))
                return new DataInfo<UserInfo> { Items = new List<UserInfo>(), Total = 0 };
            
            var foundUsers = activeDirectoryService.SearchForUserByString(searchString, searchCount).Select(
                user => new UserInfo
                {
                    Username = user.DisplayName,
                    UserSamAccount = user.SamAccountName,
                }).ToList();

            return new DataInfo<UserInfo> { Items = foundUsers, Total = foundUsers.Count() };
        }
    }
}
