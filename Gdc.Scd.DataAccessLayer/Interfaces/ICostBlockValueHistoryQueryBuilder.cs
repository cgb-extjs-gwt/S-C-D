using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockValueHistoryQueryBuilder
    {
        SelectJoinSqlHelper BuildSelectHistoryValueQuery(
            HistoryContext historyContext, 
            IEnumerable<BaseColumnInfo> addingSelectColumns = null);

        TQuery BuildJoinHistoryValueQuery<TQuery>(
            HistoryContext historyContext, 
            TQuery query, 
            JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>;

        SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(
            CostBlockHistory history, 
            TQuery query, 
            InputLevelJoinType inputLevelJoinType = InputLevelJoinType.HistoryContext, 
            IEnumerable<JoinInfo> joinInfos = null,
            long? historyValueId = null,
            IDictionary<string, IEnumerable<object>> costBlockFiter = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>;

        SqlHelper BuildSelectJoinApproveHistoryValueQuery(
            CostBlockHistory history, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}
