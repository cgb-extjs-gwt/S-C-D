using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class FromSqlBuilder : BaseQuerySqlBuilder
    {
        public ISqlBuilder From { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"{this.Query.Build(context)} FROM {this.From.Build(context)}";
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var sqlBuilder in base.GetChildrenBuilders())
            {
                yield return sqlBuilder;
            }

            yield return this.From;
        }
    }
}
