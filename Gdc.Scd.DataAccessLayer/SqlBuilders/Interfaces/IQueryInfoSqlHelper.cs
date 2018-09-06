using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IQueryInfoSqlHelper
    {
        SqlHelper ByQueryInfo(QueryInfo queryInfo);
    }
}
