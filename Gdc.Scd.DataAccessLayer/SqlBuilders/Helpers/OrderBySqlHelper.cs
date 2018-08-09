using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class OrderBySqlHelper : SqlHelper, IOrderBySqlHelper<SqlHelper>
    {
        public OrderBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SqlHelper OrderBy(params OrderByInfo[] infos)
        {
            return new SqlHelper(new OrderBySqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                OrderByInfos = infos
            });
        }

        public SqlHelper OrderBy(OrderByDirection direction, params ColumnInfo[] columns)
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
    }
}
