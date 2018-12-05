using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class ValuesInfo
    {
        public IDictionary<string, long> Coordinates { get; set; }

        public IDictionary<string, object> Values { get; set; }
    }
}
