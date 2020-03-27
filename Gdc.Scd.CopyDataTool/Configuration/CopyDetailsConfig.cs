using System;
using System.Configuration;

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
        public string Country => this.GetString("country");

        [ConfigurationProperty("targetCountry", IsRequired = false)]
        public string TargetCountry => this.GetString("targetCountry");

        [ConfigurationProperty("copyManualCosts", IsRequired = false, DefaultValue = false)]
        public bool CopyManualCosts => this.GetBool("copyManualCosts");

        [ConfigurationProperty("copyCostBlocks", IsRequired = false, DefaultValue = false)]
        public bool CopyCostBlocks => this.GetBool("copyCostBlocks");

        [ConfigurationProperty("exludedWgs", IsRequired = false)]
        public string ExcludedWgs => this["exludedWgs"] == null ? String.Empty : (string)this["exludedWgs"];

        [ConfigurationProperty("removeTargetPortfolio", IsRequired = false, DefaultValue = false)]
        public bool RemoveTargetPortfolio => this.GetBool("removeTargetPortfolio");

        public bool HasTargetCountry => !string.IsNullOrEmpty(this.TargetCountry);

        public bool HasCountry => !string.IsNullOrEmpty(this.Country);

        public bool HasExcludedWgs => !string.IsNullOrEmpty(this.ExcludedWgs);

        public string GetTargetCountry(string defaultCountry)
        {
            return this.HasTargetCountry ? this.TargetCountry : defaultCountry;
        }

        public string[] GetExcludedWgs()
        {
            return this.ExcludedWgs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string GetString(string name)
        {
            var value = (string)this[name];

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private bool GetBool(string name)
        {
            var value = this[name];

            return value != null && (bool)value;
        }
    }
}
