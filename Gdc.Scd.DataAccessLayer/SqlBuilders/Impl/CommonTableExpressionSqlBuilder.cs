using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class CommonTableExpressionSqlBuilder : BaseQuerySqlBuilder
    {
        public IEnumerable<WithQuery> WithQueries { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var queries = 
                string.Join(
                    $",{Environment.NewLine}", 
                    this.WithQueries.Select(query => this.BuildQuery(query, context)));

            return $"WITH {queries}{Environment.NewLine}{this.Query.Build(context)}";
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return base.GetChildrenBuilders().Concat(this.WithQueries.Select(query => query.Query));
        }

        private string BuildQuery(WithQuery query, SqlBuilderContext context)
        {
            var columns = string.Join(", ", query.ColumnNames);

            return 
                $"{query.Name} ({columns}) {Environment.NewLine} AS{Environment.NewLine}" + 
                $"({Environment.NewLine}{query.Query.Build(context)}{Environment.NewLine})";
        }
    }
}
