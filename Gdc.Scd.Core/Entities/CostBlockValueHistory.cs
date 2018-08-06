using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class CostBlockValueHistory
    {
        public NamedId InputLevel { get; set; }

        public IDictionary<string, NamedId> Dependencies { get; set; }

        public object Value { get; set; }
    }
}
