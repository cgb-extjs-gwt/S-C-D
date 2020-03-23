using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class SetLockEscalationSqlBuilder : ISqlBuilder
    {
        public LockEscalationType LockEscalationType { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"SET (LOCK_ESCALATION = {this.LockEscalationType.ToString().ToUpper()})";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
