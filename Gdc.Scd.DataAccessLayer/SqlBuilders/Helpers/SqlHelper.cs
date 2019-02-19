using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

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
    }
}
