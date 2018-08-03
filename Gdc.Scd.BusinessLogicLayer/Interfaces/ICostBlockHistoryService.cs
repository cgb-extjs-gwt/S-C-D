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

        Task<IEnumerable<CostBlockHistoryApprovalDto>> GetDtoHistoriesForApproval(CostBlockHistoryFilter filter);

        Task Save(CostEditorContext context, IEnumerable<EditItem> editItems, bool isApproved);

        Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(CostBlockHistory history);

        Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(long costBlockHistoryId);

        Task Approve(long historyId);

        void Reject(long historyId);
    }
}
