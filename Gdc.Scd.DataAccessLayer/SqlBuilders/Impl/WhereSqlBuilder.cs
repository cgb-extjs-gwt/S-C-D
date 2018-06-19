using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class WhereSqlBuilder : BaseSqlBuilder
    {
        public ISqlBuilder Condition { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var sql = this.SqlBuilder == null ? string.Empty : this.SqlBuilder.Build(context);

            return $"{sql} WHERE {this.Condition.Build(context)}";
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
