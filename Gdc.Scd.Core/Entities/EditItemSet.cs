using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class EditItemSet
    {
        public IEnumerable<EditItem> EditItems { get; set; }

        public IDictionary<string, long[]> CoordinateFilter { get; set; }
    }
}
