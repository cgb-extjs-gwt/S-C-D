using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockValueHistoryRepository
    {
        Task Save(CostBlockHistory history, IEnumerable<EditItem> editItems, IDictionary<string, long[]> relatedItems);

        Task<IEnumerable<CostBlockValueHistory>> GetByCostBlockHistory(CostBlockHistory history);

        Task<int> Approve(CostBlockHistory history);
    }
}
