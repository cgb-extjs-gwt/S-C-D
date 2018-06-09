using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Meta.Entities
{
    public class CostBlockMeta : BaseMeta
    {
        public List<string> ApplicationIds { get; set; }

        public List<CostElementMeta> CostElements { get; set; }
    }
}
