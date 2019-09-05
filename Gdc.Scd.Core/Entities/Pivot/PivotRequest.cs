using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Entities.Pivot
{
    public class PivotRequest
    {
        public string KeysSeparator { get; set; }

        public string GrandTotalKey { get; set; }

        public RequestAxisItem[] LeftAxis { get; set; }

        public RequestAxisItem[] TopAxis { get; set; }

        public RequestAxisItem[] Aggregate { get; set; }

        public IEnumerable<RequestAxisItem> GetAllAxisItems()
        {
            var allAxisItems = this.LeftAxis ?? Enumerable.Empty<RequestAxisItem>();

            if (this.TopAxis != null)
            {
                allAxisItems = allAxisItems.Concat(this.TopAxis);
            }

            return allAxisItems;
        }
    }
}
