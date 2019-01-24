using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public class ConsolePrincipleProvider : IPrincipalProvider
    {
        public IPrincipal GetCurrenctPrincipal()
        {
            var identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal;
        }
    }
}
