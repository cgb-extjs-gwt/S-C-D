using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class TableViewInfo
    {
        public RecordInfo RecordInfo { get; set; }

        public IDictionary<string, IEnumerable<NamedId>> References { get; set; }
    }
}
