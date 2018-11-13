using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DropTableSqlBuilder : TableSqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            return $"DROP TABLE {base.Build(context)}";
        }
    }
}
