using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class FromSqlBuilder : BaseSqlBuilder
    {
        public ISqlBuilder From { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return $"{this.SqlBuilder.Build(context)} FROM {this.From.Build(context)}";
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
