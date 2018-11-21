using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockHistoryService : IReadingDomainService<CostBlockHistory>
    {
        IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryFilter filter);

        IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryState state);

        IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryFilter filter, CostBlockHistoryState state);

        //Task<IEnumerable<HistoryItem>> GetHistoryItems(CostEditorContext context, long editItemId, QueryInfo queryInfo = null);

        //Task<IEnumerable<HistoryItem>> GetHistoryItems(CostElementIdentifier costElementId, IDictionary<string, long> coordinates, QueryInfo queryInfo = null);

        Task<IEnumerable<HistoryItem>> GetHistoryItems(HistoryContext historyContext, IDictionary<string, long[]> filter, QueryInfo queryInfo = null);

        Task Save(HistoryContext context, IEnumerable<EditItem> editItems, ApprovalOption approvalOption, IDictionary<string, long[]> filter);

        //Task Save(IEnumerable<EditInfo> editInfos, ApprovalOption approvalOption);

        void Save(CostBlockHistory history, ApprovalOption approvalOption);

        CostBlockHistory SaveAsApproved(long historyId);

        CostBlockHistory SaveAsRejected(long historyId, string rejectedMessage);
    }
}
