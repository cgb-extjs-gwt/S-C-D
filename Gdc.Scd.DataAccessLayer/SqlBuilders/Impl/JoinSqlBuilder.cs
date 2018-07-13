using System.Collections.Generic;
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
            string result;

            var sql = this.SqlBuilder.Build(context);
            var type = this.Type.ToString().ToUpper();
            var table = this.Table.Build(context);

            switch (this.Type)
            {
                case JoinType.Cross:
                    result = $"{sql} CROSS JOIN {table}";
                    break;

                default:
                    var condition = this.Condition.Build(context);

                    result = $"{sql} {type} JOIN {table} ON {condition}";
                    break;
            }

            return result;
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var builder in base.GetChildrenBuilders())
            {
                yield return builder;
            }

            if (this.Condition != null)
            {
                yield return this.Condition;
            }

            yield return this.Table;
        }
    }
}
