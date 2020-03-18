using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class HistoryContext
    {
        public CostBlockHistory History { get; set; }

        public IEnumerable<EditItem> EditItems { get; set; }

        public IDictionary<string, long[]> RelatedItems { get; set; }
    }
}
