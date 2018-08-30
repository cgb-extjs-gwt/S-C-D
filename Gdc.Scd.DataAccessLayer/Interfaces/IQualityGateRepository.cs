using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateRepository
    {
        Task<IEnumerable<CostBlockValueHistory>> Check(
            HistoryContext historyContext, 
            IEnumerable<EditItem> editItems, 
            IDictionary<string, IEnumerable<object>> costBlockFilter);

        Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetailQualityGate(CostBlockHistory history, long historyValueId);
    }
}
