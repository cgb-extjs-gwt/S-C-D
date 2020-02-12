using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Tests.Common.Entities;
using System.Security.Principal;

namespace Gdc.Scd.Tests.Common.Impl
{
    public class FakePrincipalProvider : IPrincipalProvider
    {
        public static Principal CurrentPrincipal { get; set; }

        public IPrincipal GetCurrenctPrincipal()
        {
            return CurrentPrincipal;
        }
    }
}
