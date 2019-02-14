using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class EditContext
    {
        public CostElementContext Context { get; set; }

        public IEnumerable<EditItemSet> EditItemSets { get; set; }
    }
}
