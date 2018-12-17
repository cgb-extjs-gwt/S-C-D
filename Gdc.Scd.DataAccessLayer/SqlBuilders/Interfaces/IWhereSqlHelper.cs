using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IWhereSqlHelper<out TResult>
    {
        TResult Where(ISqlBuilder condition);

        TResult Where(ConditionHelper condition);

        TResult Where(IEnumerable<ConditionHelper> conditions);

        TResult Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null);

        TResult Where(IDictionary<ColumnInfo, IEnumerable<object>> filter);
    }
}
