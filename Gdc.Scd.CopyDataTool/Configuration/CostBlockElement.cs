using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.CopyDataTool.Configuration
{
    public class CostBlockElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string) base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("scheme", IsKey = false, DefaultValue = MetaConstants.HardwareSchema)]
        public string Schema
        {
            get => (string)base["scheme"];
            set => base["scheme"] = value;
        }

    }
}
