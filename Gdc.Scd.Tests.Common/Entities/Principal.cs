using System;
using System.Security.Principal;

namespace Gdc.Scd.Tests.Common.Entities
{
    public class Principal : IPrincipal
    {
        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}
