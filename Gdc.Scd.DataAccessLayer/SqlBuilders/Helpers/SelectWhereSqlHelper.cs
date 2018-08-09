using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectWhereSqlHelper : SqlHelper, IGroupBySqlHelper<SelectGroupBySqlHelper>, IOrderBySqlHelper<SqlHelper>
    {
        private readonly GroupBySqlHelper groupByHelper;

        private readonly OrderBySqlHelper orderBySqlHelper;

        public SelectWhereSqlHelper(ISqlBuilder sqlBuilder)
            : base(sqlBuilder)
        {
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
            this.orderBySqlHelper = new OrderBySqlHelper(sqlBuilder);
        }

        public SqlHelper OrderBy(params OrderByInfo[] infos)
        {
            return this.orderBySqlHelper.OrderBy(infos);
        }

        public SqlHelper OrderBy(OrderByDirection direction, params ColumnInfo[] columns)
        {
            return this.orderBySqlHelper.OrderBy(direction, columns);
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.groupByHelper.GroupBy(columns);
        }
    }
}
