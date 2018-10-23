using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseCostElementMeta : BaseMeta
    {
        public DependencyMeta Dependency { get; set; }

        public string Description { get; set; }

        public MetaCollection<InputLevelMeta> InputLevels { get; set; }

        public InputLevelMeta RegionInput { get; set; }

        public InputType InputType { get; set; }

        public IDictionary<string, string> TypeOptions { get; set; }
    }
}
