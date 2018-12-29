using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class RecordInfo
    {
        public IEnumerable<string> Coordinates { get; set; }

        public IEnumerable<DataInfo> Data { get; set; }

        public IEnumerable<AdditionalData> AdditionalData { get; set; }
    }
}
