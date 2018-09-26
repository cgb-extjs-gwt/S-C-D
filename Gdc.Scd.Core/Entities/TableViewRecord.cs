using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class TableViewRecord
    {
        public Dictionary<string, long> Ids { get; set; }

        public Dictionary<string, object> Data { get; set; }

        public TableViewRecord()
        {
            this.Ids = new Dictionary<string, long>();
            this.Data = new Dictionary<string, object>();
        }
    }
}
