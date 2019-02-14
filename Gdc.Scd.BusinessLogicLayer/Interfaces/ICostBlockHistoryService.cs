﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockHistoryService : IReadingDomainService<CostBlockHistory>
    {
        IQueryable<CostBlockHistory> GetByFilter(BundleFilter filter);

        IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryState state);

        IQueryable<CostBlockHistory> GetByFilter(BundleFilter filter, CostBlockHistoryState state);

        Task<DataInfo<HistoryItemDto>> GetHistory(CostElementContext historyContext, IDictionary<string, long[]> filter, QueryInfo queryInfo = null);

        Task Save(CostElementContext context, IEnumerable<EditItem> editItems, ApprovalOption approvalOption, IDictionary<string, long[]> filter, EditorType editorType);

        void Save(CostBlockHistory history, ApprovalOption approvalOption);

        CostBlockHistory SaveAsApproved(long historyId);

        CostBlockHistory SaveAsRejected(long historyId, string rejectedMessage);
    }
}
