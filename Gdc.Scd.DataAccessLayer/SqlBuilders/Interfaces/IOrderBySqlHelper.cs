using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IOrderBySqlHelper<out TResult>
    {
        TResult OrderBy(params OrderByInfo[] infos);

        TResult OrderBy(OrderByDirection direction, params ColumnInfo[] columns);
    }
}
