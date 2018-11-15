using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockValueHistoryRepository
    {
        Task Save(CostBlockHistory history, IEnumerable<EditItem> editItems, IDictionary<string, long[]> relatedItems);

        Task<IEnumerable<BundleDetail>> GetApproveBundleDetail(
            CostBlockHistory history, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        Task<IEnumerable<HistoryItem>> GetHistory(HistoryContext historyContext, IDictionary<string, long[]> filter, QueryInfo queryInfo = null);

        Task<int> Approve(CostBlockHistory history);
    }
}
