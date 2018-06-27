using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectWhereSqlHelper : SqlHelper, IGroupBySqlHelper<SelectGroupBySqlHelper>
    {
        private GroupBySqlHelper groupByHelper;

        public SelectWhereSqlHelper(ISqlBuilder sqlBuilder)
            : base(sqlBuilder)
        {
            this.Init(sqlBuilder);
        }

        public SelectWhereSqlHelper(SqlHelper sqlHelper)
            : base(sqlHelper)
        {
            this.Init(sqlHelper.ToSqlBuilder());
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.groupByHelper.GroupBy(columns);
        }

        private void Init(ISqlBuilder sqlBuilder)
        {
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
        }
    }
}
