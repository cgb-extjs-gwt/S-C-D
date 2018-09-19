using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using Gdc.Scd.BusinessLogicLayer.Helpers;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IActiveDirectoryService
    {
        ActiveDirectoryConfig Configuration { get; set; }
        bool CheckCredentials(string userName, string password, string domainName);
        DirectoryEntry GetUserByEmail(string email, string userName, string password, string domainName);
        DirectoryEntry GetUserFromForestByUsername(string userName, string password, string domainName);
        List<UserPrincipal> SearchForUserByString(string search);
        UserPrincipal FindByIdentity(string userIdentity);
    }
}