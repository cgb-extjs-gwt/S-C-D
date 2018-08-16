using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockValueHistoryRepository
    {
        Task Save(CostBlockHistory history, IEnumerable<EditItem> editItems, IDictionary<string, long[]> relatedItems);

        Task<IEnumerable<CostBlockValueHistory>> GetByCostBlockHistory(CostBlockHistory history);

        Task<IEnumerable<CostBlockHistoryValueDto>> GetCostBlockHistoryValueDto(HistoryContext historyContext, IDictionary<string, IEnumerable<object>> filter, QueryInfo queryInfo = null);

        Task<int> Approve(CostBlockHistory history);
    }
}
