using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class EditItemContext
    {
        public HistoryContext Context { get; set; }

        public EditItem[] EditItems { get; set; }

        public IDictionary<string, long[]> Filter { get; set; }
    }
}
