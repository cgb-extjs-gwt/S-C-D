using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateQueryBuilder
    {
        SqlHelper BuildQualityGateQuery(HistoryContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, IEnumerable<object>> costBlockFilter);

        SqlHelper BuildQulityGateHistoryQuery(CostBlockHistory history, long historyValueId, IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}
