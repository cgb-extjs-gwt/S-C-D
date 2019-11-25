using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool.Entities
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
