using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectGroupBySqlHelper : OrderBySqlHelper, /*IOrderBySqlHelper<OffsetFetchSqlHelper>,*/ IOffsetFetchSqlHelper<SqlHelper>, IQueryInfoSqlHelper
    {
        private readonly QueryInfoSqlHelper queryInfoSqlHelper;

        private readonly GroupBySqlHelper groupByHelper;

        public SelectGroupBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.queryInfoSqlHelper = new QueryInfoSqlHelper(sqlBuilder);
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.groupByHelper.GroupBy(columns);
        }

        public SqlHelper ByQueryInfo(QueryInfo queryInfo)
        {
            return this.queryInfoSqlHelper.ByQueryInfo(queryInfo);
        }
    }
}
