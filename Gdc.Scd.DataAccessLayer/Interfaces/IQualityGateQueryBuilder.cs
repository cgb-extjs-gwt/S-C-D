﻿using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateQueryBuilder
    {
        SqlHelper BuildQualityGateQuery(HistoryContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, IEnumerable<object>> costBlockFilter);

        SqlHelper BuildQualityGateQuery(CostBlockHistory history, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        SqlHelper BuildQulityGateApprovalQuery(CostBlockHistory history, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}