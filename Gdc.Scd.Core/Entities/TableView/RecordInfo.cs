using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class RecordInfo
    {
        public IEnumerable<FieldInfo> Coordinates { get; set; }

        public IEnumerable<FieldInfo> Data { get; set; }
    }
}
