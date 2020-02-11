using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseCostBlockMeta<TCostElement> : BaseMeta 
        where TCostElement : BaseMeta
    {
        public string[] ApplicationIds { get; set; }

        public MetaCollection<TCostElement> CostElements { get; set; }
    }
}
