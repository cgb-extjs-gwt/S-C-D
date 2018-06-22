using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IGroupBySqlHelper<TResult>
    {
        TResult GroupBy(params ColumnInfo[] columns);
    }
}
