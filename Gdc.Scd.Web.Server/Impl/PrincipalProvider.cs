using System.Security.Principal;
using System.Web;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Web.Server.Impl
{
    public class PrincipalProvider : IPrincipalProvider
    {
        public IPrincipal GetCurrenctPrincipal()
        {
            return HttpContext.Current.User;
        }
    }
}