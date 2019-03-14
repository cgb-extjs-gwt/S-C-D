using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockQueryBuilder
    {
        OrderBySqlHelper BuildSelectQuery(CostBlockSelectQueryData queryData);
    }
}
