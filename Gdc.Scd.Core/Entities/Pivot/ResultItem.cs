using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.Pivot
{
    public class ResultItem
    {
        public string LeftKey { get; set; }

        public string TopKey { get; set; }

        public Dictionary<string, object> Values { get; set; }
    }
}
