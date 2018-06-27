using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class JoinSqlBuilder : BaseSqlBuilder
    {
        public ISqlBuilder Condition { get; set; }

        public ISqlBuilder Table { get; set; }

        public JoinType Type { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var sql = this.SqlBuilder.Build(context);
            var type = this.Type.ToString().ToUpper();
            var table = this.Table.Build(context);
            var condition = this.Condition.Build(context);

            return $"{sql} {type} JOIN {table} ON {condition}";
        }
    }
}
