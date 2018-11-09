using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class IntoSqlBuilder : TableSqlBuilder
    {
        public ISqlBuilder Query { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"{this.Query.Build(context)} INTO {base.Build(context)}";
        }
    }
}
