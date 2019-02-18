using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.Entities;
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

        public string ToSql()
        {
            return this.sqlBuilder.Build(new SqlBuilderContext());
        }

        public ISqlBuilder ToSqlBuilder()
        {
            return this.sqlBuilder;
        }

        public IEnumerable<CommandParameterInfo> GetParameters()
        {
            var context = new SqlBuilderContext();

            this.sqlBuilder.Build(context);

            return context.GetParameters();
        }
    }
}
