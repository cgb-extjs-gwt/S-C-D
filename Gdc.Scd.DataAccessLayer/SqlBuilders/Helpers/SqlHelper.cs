using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SqlHelper
    {
        private readonly ISqlBuilder sqlBuilder;

        public SqlHelper(ISqlBuilder sqlBuilder)
        {
            this.sqlBuilder = sqlBuilder;
        }

        public SqlHelper(SqlHelper helper)
            : this(helper.sqlBuilder)
        {
        }

        public ISqlBuilder ToSqlBuilder()
        {
            return this.sqlBuilder;
        }

        public QueryData ToQueryData()
        {
            var context = new SqlBuilderContext();

            return new QueryData
            {
                Sql = this.sqlBuilder.Build(context),
                Parameters = context.GetParameters()
            };
        }

        public string ToSqlScript()
        {
            var queryData = this.ToQueryData();
            var paramsQueries = queryData.Parameters.Select(param => new DeclareSqlBuiler
            {
                Name = param.Name,
                DefaultValue = param.Value
            });

            var queries = new List<ISqlBuilder>(paramsQueries)
            {
                new RawSqlBuilder(queryData.Sql)
            };

            return Sql.Queries(queries).ToQueryData().Sql;
        }
    }
}
