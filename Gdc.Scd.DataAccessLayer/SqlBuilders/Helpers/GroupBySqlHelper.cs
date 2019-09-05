using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class GroupBySqlHelper : OrderBySqlHelper, IGroupBySqlHelper<OrderBySqlHelper>
    {
        public GroupBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public OrderBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.GroupBy(GroupByType.Simple, columns);
        }

        public OrderBySqlHelper GroupBy(GroupByType type, params ColumnInfo[] columns)
        {
            return new OrderBySqlHelper(new GroupBySqlBuilder
            {
                Query = this.ToSqlBuilder(),
                Columns = columns,
                Type = type
            });
        }
    }
}
