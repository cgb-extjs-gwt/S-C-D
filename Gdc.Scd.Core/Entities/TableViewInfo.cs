using System.Collections.Generic;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.Core.Entities
{
    public class TableViewInfo
    {
        public TableViewRecordInfo RecordInfo { get; set; }

        public IDictionary<string, IEnumerable<NamedId>> Filters { get; set; }

        public IDictionary<string, IEnumerable<NamedId>> References { get; set; }
    }
}
