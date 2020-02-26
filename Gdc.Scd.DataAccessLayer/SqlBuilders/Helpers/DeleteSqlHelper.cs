using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class DeleteSqlHelper : SqlHelper, IWhereSqlHelper<SqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        public DeleteSqlHelper(ISqlBuilder sqlBuilder) : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
        }

        public SqlHelper Where(ISqlBuilder condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public SqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SqlHelper(this.whereHelper.Where(filter));
        }

        public SqlHelper Where(IDictionary<ColumnInfo, IEnumerable<object>> filter)
        {
            return new SqlHelper(this.whereHelper.Where(filter));
        }

        public SqlHelper Where(IEnumerable<ConditionHelper> conditions)
        {
            return new SqlHelper(this.whereHelper.Where(conditions));
        }
    }
}
