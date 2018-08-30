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
        SelectJoinSqlHelper BuildSelectHistoryValueQuery(HistoryContext historyContext, IEnumerable<BaseColumnInfo> addingSelectColumns = null, string valueColumnName = null);

        TQuery BuildJoinHistoryValueQuery<TQuery>(
            HistoryContext historyContext, 
            TQuery query, 
            JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>;

        SqlHelper BuildJoinHistoryValueQuery<TQuery>(
            CostBlockHistory history, 
            TQuery query, 
            JoinHistoryValueQueryOptions options = null,
            long? historyValueId = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>;

        SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(
            CostBlockHistory history, 
            TQuery query, 
            InputLevelJoinType inputLevelJoinType = InputLevelJoinType.HistoryContext, 
            IEnumerable<JoinInfo> joinInfos = null,
            long? historyValueId = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>;

        SqlHelper BuildSelectJoinHistoryValueQuery(
            CostBlockHistory history,
            string inputLevelId,
            InputLevelJoinType inputLevelJoinType,
            long? historyValueId = null,
            string valueColumnName = null);
    }
}
