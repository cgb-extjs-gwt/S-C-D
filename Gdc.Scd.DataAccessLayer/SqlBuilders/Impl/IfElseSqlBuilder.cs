using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class IfElseSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder Condition { get; set; }

        public ISqlBuilder TrueQuery { get; set; }

        public ISqlBuilder FalseQuery { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var condition = this.Condition.Build(context);
            var trueQuery = this.TrueQuery.Build(context);
            var query = $"IF {condition}{Environment.NewLine}{trueQuery}";

            if (this.FalseQuery != null)
            {
                var falseQuery = this.FalseQuery.Build(context);

                query += $"ELSE{Environment.NewLine}{falseQuery}";
            }

            return query;
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Condition;
            yield return this.TrueQuery;

            if (this.FalseQuery != null)
            {
                yield return this.FalseQuery;
            }
        }
    }
}
