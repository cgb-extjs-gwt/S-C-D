using System;

namespace Gdc.Scd.Core.Entities.Approval
{
    public class BundleFilter
    {
        public DateTime? DateTimeFrom { get; set; }

        public DateTime? DateTimeTo { get; set; }

        public string[] ApplicationIds { get; set; }

        public string[] CostBlockIds { get; set; }

        public string[] CostElementIds { get; set; }

        public long[] UserIds { get; set; }

        public CostBlockHistoryState? State { get; set; }
    }
}
