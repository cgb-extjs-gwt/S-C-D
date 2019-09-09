using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class HavingSqlBuilder : BaseQuerySqlBuilder
    {
        public ISqlBuilder Condition { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var sql = this.Query == null ? string.Empty : this.Query.Build(context);

            return $"{sql} HAVING {this.Condition.Build(context)}";
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var builder in base.GetChildrenBuilders())
            {
                yield return builder;
            }

            yield return this.Condition;
        }
    }
}
