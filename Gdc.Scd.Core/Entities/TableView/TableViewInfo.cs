using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class TableViewInfo
    {
        public RecordInfo RecordInfo { get; set; }

        public IDictionary<string, ReferenceSet> CostBlockReferences { get; set; }

        public NamedId[] RoleCodeReferences { get; set; }

        public IDictionary<string, IDictionary<long, NamedId>> DependencyItems { get; set; }

        public NamedId GetDependencyItem(string dependencyId, long dependencyItemId)
        {
            return this.DependencyItems[dependencyId][dependencyItemId];
        }

        public NamedId GetRoleCode(long id)
        {
            return this.RoleCodeReferences.First(roleCode => roleCode.Id == id);
        }

        public NamedId GetRoleCode(Record record)
        {
            return
                record.WgRoleCodeId.HasValue
                    ? this.GetRoleCode(record.WgRoleCodeId.Value)
                    : null;
        }
    }
}
