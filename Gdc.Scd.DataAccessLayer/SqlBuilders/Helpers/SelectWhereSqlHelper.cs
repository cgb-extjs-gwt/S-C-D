using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectWhereSqlHelper : GroupBySqlHelper, IWhereSqlHelper<GroupBySqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        public SelectWhereSqlHelper(ISqlBuilder sqlBuilder)
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
        }

        public GroupBySqlHelper Where(ISqlBuilder condition)
        {
            return new GroupBySqlHelper(this.whereHelper.Where(condition));
        }

        public GroupBySqlHelper Where(ConditionHelper condition)
        {
            return new GroupBySqlHelper(this.whereHelper.Where(condition));
        }

        public GroupBySqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new GroupBySqlHelper(this.whereHelper.Where(filter, tableName));
        }

        public GroupBySqlHelper Where(IDictionary<ColumnInfo, IEnumerable<object>> filter)
        {
            return new GroupBySqlHelper(this.whereHelper.Where(filter));
        }
    }
}
