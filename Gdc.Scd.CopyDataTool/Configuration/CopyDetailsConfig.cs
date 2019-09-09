using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool.Configuration
{
    public class CopyDetailsConfig : ConfigurationSection
    {
        [ConfigurationProperty("costBlocks")]
        [ConfigurationCollection(typeof(CostBlockCollection))]
        public CostBlockCollection CostBlocks => (CostBlockCollection) this["costBlocks"];

        [ConfigurationProperty("excludedCostElements")]
        [ConfigurationCollection(typeof(ExcludedCostElementsCollection))]
        public ExcludedCostElementsCollection ExcludedCostElements => (ExcludedCostElementsCollection) this["excludedCostElements"];

        [ConfigurationProperty("editUser", IsRequired = false)]
        public string EditUser => this["editUser"] == null ? String.Empty : (string) this["editUser"];

        [ConfigurationProperty("country", IsRequired = false)]
        public string Country => this["country"] == null ? String.Empty : (string)this["country"];

        [ConfigurationProperty("copyManualCosts", IsRequired = false, DefaultValue = false)]
        public bool CopyManualCosts => this["copyManualCosts"] != null && (bool)this["copyManualCosts"];

        [ConfigurationProperty("exludedWgs", IsRequired = false)]
        public string ExcludedWgs => this["exludedWgs"] == null ? String.Empty : (string) this["exludedWgs"];
    }
}
