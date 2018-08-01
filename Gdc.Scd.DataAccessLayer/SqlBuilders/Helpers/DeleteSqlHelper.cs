using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class DeleteSqlHelper : SqlHelper, IWhereSqlHelper<SqlHelper>
    {
        private const string ID_COL = "Id";

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

        public SqlHelper WhereId(IEnumerable<long> items)
        {
            return Where(new Dictionary<string, IEnumerable<object>> { { ID_COL, items.Cast<object>() } });
        }

        public SqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SqlHelper(this.whereHelper.Where(filter));
        }
    }
}
