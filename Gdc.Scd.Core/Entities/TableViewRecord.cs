using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class TableViewRecord
    {
        public Dictionary<string, NamedId> Coordinates { get; set; }

        public Dictionary<string, ValueCount> Data { get; set; }

        public TableViewRecord()
        {
            this.Coordinates = new Dictionary<string, NamedId>();
            this.Data = new Dictionary<string, ValueCount>();
        }
    }
}
