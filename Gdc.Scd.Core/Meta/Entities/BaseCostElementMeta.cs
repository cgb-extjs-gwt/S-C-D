using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseCostElementMeta<TInputLevel> : BaseMeta
        where TInputLevel : InputLevelMeta
    {
        public DependencyMeta Dependency { get; set; }

        public string Description { get; set; }

        public virtual IEnumerable<TInputLevel> InputLevels { get; }

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

        public TInputLevel GetInputLevel(string inputLevelId)
        {
            return this.InputLevels.FirstOrDefault(inputLevel => inputLevel.Id == inputLevelId);
        }

        public bool HasInputLevel(string inputLevelId)
        {
            return this.GetInputLevel(inputLevelId) != null;
        }
    }
}
