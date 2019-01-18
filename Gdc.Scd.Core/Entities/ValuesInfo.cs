using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class ValuesInfo
    {
        public IDictionary<string, IEnumerable<object>> Filter { get; set; }

        public IDictionary<string, object> Values { get; set; }
    }
}
