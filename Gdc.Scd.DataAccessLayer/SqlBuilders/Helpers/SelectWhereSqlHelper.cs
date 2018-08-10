using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectWhereSqlHelper : SelectGroupBySqlHelper, IWhereSqlHelper<SelectGroupBySqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        public SelectWhereSqlHelper(ISqlBuilder sqlBuilder)
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
        }

        public SelectGroupBySqlHelper Where(ISqlBuilder condition)
        {
            return new SelectGroupBySqlHelper(this.whereHelper.Where(condition));
        }

        public SelectGroupBySqlHelper Where(ConditionHelper condition)
        {
            return new SelectGroupBySqlHelper(this.whereHelper.Where(condition));
        }

        public SelectGroupBySqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SelectGroupBySqlHelper(this.whereHelper.Where(filter, tableName));
        }
    }
}
