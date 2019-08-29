using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    internal static class PivotGridSqlExtenstions
    {
        public static SelectJoinSqlHelper From(this SelectIntoSqlHelper query, BaseEntityMeta meta, SqlHelper customQuery)
        {
            return customQuery == null 
                ? query.From(meta) 
                : query.FromQuery(customQuery, meta.FullName);
        }
    }
}
