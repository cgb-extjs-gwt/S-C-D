using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateRepository
    {
        Task<IEnumerable<CostBlockValueHistory>> Check(HistoryContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, long[]> costBlockFilter);

        Task<IEnumerable<CostBlockValueHistory>> Check(CostBlockHistory history, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetailQualityGate(
            CostBlockHistory history, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}
