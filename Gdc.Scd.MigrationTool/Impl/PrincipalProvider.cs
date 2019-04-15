using System.Security.Principal;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.MigrationTool.Entities;

namespace Gdc.Scd.MigrationTool.Impl
{
    public class PrincipalProvider : IPrincipalProvider
    {
        public static Principal CurrentPricipal { get; set; }

        public IPrincipal GetCurrenctPrincipal()
        {
            return CurrentPricipal;
        }
    }
}
