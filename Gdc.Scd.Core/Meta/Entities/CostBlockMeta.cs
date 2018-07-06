using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockMeta : CostAtomMeta
    {
        public IEnumerable<string> ApplicationIds { get; set; }
    }
}
