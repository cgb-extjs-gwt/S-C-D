using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Tests.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
