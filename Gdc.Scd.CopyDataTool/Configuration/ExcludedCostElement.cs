using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool.Configuration
{
    public class ExcludedCostElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("costBlock", IsKey = false, IsRequired = true)]
        public string CostBlock
        {
            get => (string)base["costBlock"];
            set => base["costBlock"] = value;
        }
    }
}
