using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IGroupBySqlHelper<out TResult>
    {
        TResult GroupBy(params string[] columnNames);

        TResult GroupBy(params ColumnInfo[] columns);

        TResult GroupBy(GroupByType type, params ColumnInfo[] columns);
    }
}
