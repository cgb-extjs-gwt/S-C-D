using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class OrderBySqlHelper : SqlHelper, IOrderBySqlHelper<UnionSqlHelper>, IQueryInfoSqlHelper
    {
        public OrderBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public UnionSqlHelper OrderBy(params OrderByInfo[] infos)
        {
            return new UnionSqlHelper(new OrderBySqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                OrderByInfos = infos
            });
        }

        public UnionSqlHelper OrderBy(SortDirection direction, params ColumnInfo[] columns)
        {
            var orderByInfos = columns.Select(column => new OrderByInfo
            {
                Direction = direction,
                SqlBuilder = new ColumnSqlBuilder
                {
                    Table = column.TableName,
                    Name = column.Name
                }
            }).ToArray();

            return this.OrderBy(orderByInfos);
        }

        public SqlHelper ByQueryInfo(QueryInfo queryInfo)
        {
            SqlHelper result = this;

            if (queryInfo != null)
            {
                var orderByQuery = this.OrderBy(queryInfo.Sort.Direction, new ColumnInfo(queryInfo.Sort.Property));
                result = queryInfo.Skip.HasValue || queryInfo.Take.HasValue
                    ? orderByQuery.OffsetFetch(queryInfo.Skip ?? 0, queryInfo.Take)
                    : orderByQuery;
            }

            return result;
        }
    }
}
