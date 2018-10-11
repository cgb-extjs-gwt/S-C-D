using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class TableViewRecordInfo
    {
        public IEnumerable<FieldInfo> Coordinates { get; set; }

        public IEnumerable<FieldInfo> Data { get; set; }
    }
}
