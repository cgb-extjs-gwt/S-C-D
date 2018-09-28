using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public class ActiveDirectoryConfig
    {
        public string ForestName { get; set; }
        public string DefaultDomain { get; set; }
        public string AdServiceAccount { get; set; }
        public string AdServicePassword { get; set; }
    }
}
