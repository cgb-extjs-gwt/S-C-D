using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class CostBlockQueryHelper
    {
        public static ConditionHelper BuildNotDeletedCondition(CostBlockEntityMeta meta, string tableName = null)
        {
            return SqlOperators.IsNull(meta.DeletedDateField.Name, tableName);
        }

        public static ConditionHelper BuildFilterConditionn(
            CostBlockEntityMeta meta, 
            IDictionary<string, IEnumerable<object>> filter = null, 
            string tableName = null, 
            string paramPrefix = null)
        {
            ConditionHelper result;

            var notDeletedCondition = BuildNotDeletedCondition(meta, tableName);

            if (filter != null && filter.Count > 0)
            {
                result = ConditionHelper.AndStatic(filter, tableName, paramPrefix).And(notDeletedCondition);
            }
            else
            {
                result = notDeletedCondition;
            }

            return result;
        }

        public static T WhereNotDeleted<T>(
            this IWhereSqlHelper<T> query, 
            CostBlockEntityMeta meta, 
            IDictionary<string, IEnumerable<object>> filter, 
            string tableName = null, 
            string paramPrefix = null)
        {
            return query.Where(BuildFilterConditionn(meta, filter, tableName, paramPrefix));
        }

        public static T WhereNotDeleted<T>(this IWhereSqlHelper<T> query, CostBlockEntityMeta meta, string tableName = null)
        {
            return query.Where(BuildNotDeletedCondition(meta, tableName));
        }
    }
}
