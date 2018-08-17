using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class WithSqlBuilder : BaseSqlBuilder
    {
        public IEnumerable<WithQuery> Queries { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var queries = 
                string.Join(
                    $",{Environment.NewLine}", 
                    this.Queries.Select(query => this.BuildQuery(query, context)));

            return $"WITH {queries}{Environment.NewLine}{this.SqlBuilder.Build(context)}";
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Queries.Select(query => query.Query);
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
