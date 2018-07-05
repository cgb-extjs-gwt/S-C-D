using System;
using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class InsertQuerySqlBuilder : BaseSqlBuilder
    {
        public ISqlBuilder Query { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return string.Concat(
                this.SqlBuilder.Build(context),
                Environment.NewLine,
                this.Query.Build(context));
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var builder in base.GetChildrenBuilders())
            {
                yield return builder;
            }

            yield return this.Query;
        }
    }
}
