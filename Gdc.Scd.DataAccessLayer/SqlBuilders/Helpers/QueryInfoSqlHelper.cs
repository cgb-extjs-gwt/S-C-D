using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class QueryInfoSqlHelper : SqlHelper, IQueryInfoSqlHelper
    {
        private readonly OrderBySqlHelper orderBySqlHelper;

        public QueryInfoSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.orderBySqlHelper = new OrderBySqlHelper(sqlBuilder);
        }

        public SqlHelper ByQueryInfo(QueryInfo queryInfo)
        {
            SqlHelper result = this;

            if (queryInfo != null)
            {
                var orderByQuery = this.orderBySqlHelper.OrderBy(queryInfo.Sort.Direction, new ColumnInfo(queryInfo.Sort.Property));
                result = queryInfo.Skip.HasValue || queryInfo.Take.HasValue
                    ? orderByQuery.OffsetFetch(queryInfo.Skip ?? 0, queryInfo.Take)
                    : orderByQuery;
            }

            return result;
        }
    }
}
