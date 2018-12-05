using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Admin })]
    public class UserController : BaseDomainController<User>
    {
        private readonly IActiveDirectoryService activeDirectoryService;

        public UserController(
                IDomainService<User> domainService, 
                IActiveDirectoryService activeDirectoryService
            ) : base(domainService)
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

        [HttpGet]
        public void SelectUser(string userIdentity, string _dc)
        {
            var userDirectoryEntry = activeDirectoryService.FindByIdentity(userIdentity);
            // some other behavior
        }

        [HttpGet]
        public DataInfo<User> SearchUser(string _dc, string searchString, int page = 1, int start = 0, int limit = 25)
        {
            var searchCount = Int32.Parse(ConfigurationManager.AppSettings["UsersSearchCount"]);
            if (string.IsNullOrEmpty(searchString))
                return new DataInfo<User> { Items = new List<User>(), Total = 0 };

            var foundUsers = activeDirectoryService.SearchForUserByString(searchString, searchCount).Select(
                user => new User
                {
                    Name = user.DisplayName,
                    Login = user.Sid.Translate(typeof(NTAccount)).ToString(),
                    Email = user.EmailAddress
                }).OrderBy(x => x.Name).ToList();

            return new DataInfo<User> { Items = foundUsers, Total = foundUsers.Count() };
        }
    }
}