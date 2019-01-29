using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IActiveDirectoryService
    {
        ActiveDirectoryConfig Configuration { get; set; }
        bool CheckCredentials(string userName, string password, string domainName);
        DirectoryEntry GetUserByEmail(string email, string userName, string password, string domainName);
        SearchResultCollection GetUserFromForest(string search);
        List<UserPrincipal> GetUserFromDomain(string search, int count = 5);
        List<User> SearchForUserByString(string search, int count);
        UserPrincipal FindByIdentity(string userIdentity);
    }
}