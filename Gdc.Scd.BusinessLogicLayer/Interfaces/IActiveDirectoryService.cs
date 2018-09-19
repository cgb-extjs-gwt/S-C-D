using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IActiveDirectoryService
    {
        bool CheckCredentials(string userName, string password, string domainName);
        DirectoryEntry GetUserByEmail(string email, string userName, string password, string domainName);
        DirectoryEntry GetUserFromForestByUsername(string userName, string password, string domainName);
        IEnumerable<DirectoryEntry> SearchForUserByString(string search);
        UserPrincipal FindByIdentity(string userIdentity);
    }
}