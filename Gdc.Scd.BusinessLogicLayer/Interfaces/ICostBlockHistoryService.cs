using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockHistoryService
    {
        IQueryable<CostBlockHistory> GetHistories();

        IQueryable<CostBlockHistory> GetHistories(CostBlockHistoryFilter filter);

        IQueryable<CostBlockHistory> GetHistoriesForApproval();

        IQueryable<CostBlockHistory> GetHistoriesForApproval(CostBlockHistoryFilter filter);

        Task<IEnumerable<ApprovalBundle>> GetApprovalBundles(CostBlockHistoryFilter filter);

        Task<IEnumerable<HistoryItem>> GetHistory(CostEditorContext context, long editItemId, QueryInfo queryInfo = null);

        Task Save(CostEditorContext context, IEnumerable<EditItem> editItems, ApprovalOption approvalOption);

        Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetail(CostBlockHistory history, long? historyValueId = null);

        Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetail(long costBlockHistoryId, long? historyValueId = null);

        Task Approve(long historyId);

        void Reject(long historyId, string message = null);
    }
}
