using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class EditContext
    {
        public HistoryContext Context { get; set; }

        public IEnumerable<EditItemSet> EditItemSets { get; set; }
    }
}
