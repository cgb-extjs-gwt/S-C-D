using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class GroupBySqlHelper : OrderBySqlHelper, IGroupBySqlHelper<HavingSqlHelper>
    {
        public GroupBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public HavingSqlHelper GroupBy(params string[] columnNames)
        {
            var columns = columnNames.Select(columnName => new ColumnInfo(columnName)).ToArray();

            return this.GroupBy(columns);
        }

        public HavingSqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.GroupBy(GroupByType.Simple, columns);
        }

        public HavingSqlHelper GroupBy(GroupByType type, params ColumnInfo[] columns)
        {
            return new HavingSqlHelper(new GroupBySqlBuilder
            {
                Query = this.ToSqlBuilder(),
                Columns = columns,
                Type = type
            });
        }
    }
}
