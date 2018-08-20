namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IOffsetFetchSqlHelper<out TResult>
    {
        TResult OffsetFetch(int offset, int? fetch = null);
    }
}
