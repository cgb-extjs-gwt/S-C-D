using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class ValuesInfo
    {
        public IDictionary<string, long[]> CoordinateFilter { get; set; }

        public IDictionary<string, object> Values { get; set; }
    }
}
