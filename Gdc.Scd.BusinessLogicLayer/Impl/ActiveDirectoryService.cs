using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly WindowsIdentity currentIdentity;
        private readonly string forestName;
        private readonly string defaultDomain;
        private readonly string adServiceAccount;
        private readonly string adServicePassword;
        public ActiveDirectoryService()
        {
            currentIdentity = WindowsIdentity.GetCurrent();
            this.forestName = ConfigurationManager.ConnectionStrings["AdForestName"].ConnectionString;
            this.defaultDomain = ConfigurationManager.ConnectionStrings["defaultDomain"].ConnectionString;
            this.adServiceAccount = ConfigurationManager.ConnectionStrings["AdServiceAccount"].ConnectionString;
            this.adServicePassword = ConfigurationManager.ConnectionStrings["AdServicePassword"].ConnectionString;
        }
        public bool CheckCredentials(string userName, string password, string domainName)
        {
            var nameToCheck = userName.Split('\\').Last();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, domainName, userName, password))
                {
                    var result = context.ValidateCredentials(nameToCheck, password);
                    return result;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public DirectoryEntry GetUserByEmail(string email, string userName, string password, string domainName)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainName, userName, password))
            {
                using (var user = new UserPrincipal(context))
                {
                    user.EmailAddress = string.Format("*{0}*", email);
                    var searcher = new PrincipalSearcher
                    {
                        QueryFilter = user,
                    };
                    try
                    {
                        var result = searcher.FindOne();
                        if (result == null)
                            return null;
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        return de;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        public DirectoryEntry GetUserFromForestByUsername(string userName, string password, string domainName)
        {
            if (string.IsNullOrEmpty(forestName) || string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(password))
                return null;
            var domainContext = new DirectoryContext(DirectoryContextType.Forest, forestName, userName, password);
            var currentForest = Forest.GetForest(domainContext);
            var gc = currentForest.FindGlobalCatalog();

            using (var userSearcher = gc.GetDirectorySearcher())
            {
                userSearcher.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(|(samaccountname=" + userName + ")(mail=" + userName + ")))";
                var result = userSearcher.FindOne();
                if (result == null)
                    return null;
                return result.GetDirectoryEntry();
            }
        }

        public IEnumerable<DirectoryEntry> SearchForUserByString(string search)
        {
            var searchResults = new List<DirectoryEntry>();
            using (var context = new PrincipalContext(ContextType.Domain, defaultDomain, adServiceAccount, adServicePassword))
            {
                using (var user = new UserPrincipal(context))
                {
                    user.DisplayName = string.Format("*{0}*", search);
                    var searcher = new PrincipalSearcher
                    {
                        QueryFilter = user,
                    };
                    try
                    {
                        var results = searcher.FindAll();
                        if (results == null || results.Count() == 0)
                            return null;
                        foreach (var result in results)
                        {
                            searchResults.Add(result.GetUnderlyingObject() as DirectoryEntry);
                        }
                        return searchResults;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }
        public UserPrincipal FindByIdentity(string userIdentity)
        {
            using (var context = new PrincipalContext(ContextType.Domain, defaultDomain, adServiceAccount, adServicePassword))
            {
                try
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, userIdentity);
                    return user;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}