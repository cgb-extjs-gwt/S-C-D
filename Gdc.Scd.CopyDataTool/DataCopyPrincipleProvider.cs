using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.CopyDataTool.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.CopyDataTool
{
    public class DataCopyPrincipleProvider : IPrincipalProvider
    {
        private readonly IPrincipal _currentPrinciple;

        public DataCopyPrincipleProvider(string user)
        {
            _currentPrinciple = new Principal
            {
                Identity = new Identity
                {
                    Name = user,
                    IsAuthenticated = true
                }
            };
        }

        public IPrincipal GetCurrenctPrincipal()
        {
            return _currentPrinciple;
        }
    }
}
