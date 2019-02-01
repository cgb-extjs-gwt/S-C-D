using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateQueryBuilder
    {
        SqlHelper BuildQualityGateQuery(
            CostElementContext historyContext, 
            IEnumerable<EditItem> editItems, 
            IDictionary<string, IEnumerable<object>> costBlockFilter, 
            bool useCountryGroupCheck);

        SqlHelper BuildQualityGateQuery(
            CostBlockHistory history, 
            bool useCountryGroupCheck, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        SqlHelper BuildQulityGateApprovalQuery(
            CostBlockHistory history, 
            bool useCountryGroupCheck, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}
