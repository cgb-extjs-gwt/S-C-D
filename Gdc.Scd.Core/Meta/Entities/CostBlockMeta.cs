using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockMeta : BaseDomainMeta
    {
        public IEnumerable<string> ApplicationIds { get; set; }

        public IEnumerable<CostElementMeta> CostElements { get; set; }

        public CostElementMeta GetCostElement(string id)
        {
            return this.CostElements.FirstOrDefault(costElement => costElement.Id == id);
        }
    }
}
