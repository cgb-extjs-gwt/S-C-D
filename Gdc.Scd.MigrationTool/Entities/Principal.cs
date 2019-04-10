using System;
using System.Security.Principal;

namespace Gdc.Scd.MigrationTool.Entities
{
    public class Principal : IPrincipal
    {
        IIdentity IPrincipal.Identity => this.Identity;

        public Identity Identity { get; set; }

        bool IPrincipal.IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}
