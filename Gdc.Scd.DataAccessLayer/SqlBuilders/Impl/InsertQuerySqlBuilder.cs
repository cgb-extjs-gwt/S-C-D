using System;
using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class InsertQuerySqlBuilder : BaseQuerySqlBuilder
    {
        public ISqlBuilder InsertQuery { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            return string.Concat(
                this.Query.Build(context),
                Environment.NewLine,
                this.InsertQuery.Build(context));
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var builder in base.GetChildrenBuilders())
            {
                yield return builder;
            }

            yield return this.InsertQuery;
        }
    }
}
