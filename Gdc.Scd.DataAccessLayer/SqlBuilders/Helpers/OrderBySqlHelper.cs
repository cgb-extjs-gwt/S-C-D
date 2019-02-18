using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class OrderBySqlHelper : UnionSqlHelper, IOrderBySqlHelper<UnionSqlHelper>, IQueryInfoSqlHelper
    {
        public OrderBySqlHelper(ISqlBuilder sqlBuilder)
            : base(sqlBuilder)
        {
        }

        public UnionSqlHelper OrderBy(params OrderByInfo[] infos)
        {
            return new UnionSqlHelper(new OrderBySqlBuilder
            {
                Query = this.ToSqlBuilder(),
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
            if (queryInfo == null)
            {
                return this;
            }

            if (queryInfo.Skip.HasValue || queryInfo.Take.HasValue)
            {
                int skip = queryInfo.Skip ?? 0;
                int take = queryInfo.Take ?? 0;

                var sb = new System.Text.StringBuilder();

                var field = queryInfo.Sort.Property;
                var dir = queryInfo.Sort.Direction;

                sb.Append(@"select * from (select t.*, ROW_NUMBER() OVER (order by [").Append(field).Append("] ").Append(dir).Append(") AS rownum from (")
                    .Append(ToSql())
                    .Append(")t)T1 where rownum between ").Append(skip).Append(" and ").Append(take + skip);

                return new SqlHelper(new RawSqlBuilder(sb.ToString(), ToSqlBuilder().GetChildrenBuilders()));
            }
            else
            {
                return OrderBy(queryInfo.Sort.Direction, new ColumnInfo(queryInfo.Sort.Property));
            }
        }
    }
}
