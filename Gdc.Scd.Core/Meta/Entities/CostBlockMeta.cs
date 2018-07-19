using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockMeta : BaseDomainMeta
    {
        public IEnumerable<string> ApplicationIds { get; set; }

        public MetaCollection<CostElementMeta> CostElements { get; set; }

        public IEnumerable<InputLevelMeta> GetInputLevels()
        {
            return this.CostElements.SelectMany(costElement => costElement.InputLevels).Distinct();
        }
    }
}
