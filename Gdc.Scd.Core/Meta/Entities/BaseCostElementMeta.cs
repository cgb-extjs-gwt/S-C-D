using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseCostElementMeta<TInputLevel> : BaseMeta
        where TInputLevel : InputLevelMeta
    {
        public DependencyMeta Dependency { get; set; }

        public string Description { get; set; }

        public MetaCollection<TInputLevel> InputLevels { get; set; }

        public InputLevelMeta RegionInput { get; set; }

        public IDictionary<string, string> TypeOptions { get; set; }

        public InputType InputType { get; set; }

        public bool IsCountryCurrencyCost
        {
            get
            {
                return this.GetOptionsType() == "CountryCurrencyCost";
            }
        }

        public string GetOptionsType()
        {
            string type = null;

            if (this.TypeOptions != null)
            {
                this.TypeOptions.TryGetValue("Type", out type);
            }

            return type;
        }
    }
}
