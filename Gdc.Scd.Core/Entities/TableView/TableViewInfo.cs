using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class TableViewInfo
    {
        public RecordInfo RecordInfo { get; set; }

        public IDictionary<string, ReferenceSet> CostBlockReferences { get; set; }

        public IEnumerable<NamedId> RoleCodeReferences { get; set; }

        public IDictionary<string, IDictionary<long, NamedId>> DependencyItems { get; set; }
    }
}
