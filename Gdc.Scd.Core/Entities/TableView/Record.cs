using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class Record
    {
        public Dictionary<string, NamedId> Coordinates { get; set; }

        public Dictionary<string, ValueCount> Data { get; set; }

        public Dictionary<string, string> AdditionalData { get; set; }

        public Record()
        {
            this.Coordinates = new Dictionary<string, NamedId>();
            this.Data = new Dictionary<string, ValueCount>();
            this.AdditionalData = new Dictionary<string, string>();
        }
    }
}
