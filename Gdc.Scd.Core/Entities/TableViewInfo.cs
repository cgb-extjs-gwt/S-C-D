using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class TableViewInfo
    {
        public TableViewRecordInfo RecordInfo { get; set; }

        public IDictionary<string, IEnumerable<NamedId>> References { get; set; }
    }
}
