using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IFromSqlHelper<out TResult>
    {
        TResult From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null);

        TResult From(BaseEntityMeta meta, string alias = null);

        TResult FromQuery(ISqlBuilder query);

        TResult FromQuery(SqlHelper sqlHelper);
    }
}
