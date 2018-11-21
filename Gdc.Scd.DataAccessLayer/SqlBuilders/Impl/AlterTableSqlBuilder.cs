using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AlterTableSqlBuilder : TableSqlBuilder
    {
        public ISqlBuilder Query { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"ALTER TABLE {base.Build(context)} {Query.Build(context)} ";
        }
    }
}
