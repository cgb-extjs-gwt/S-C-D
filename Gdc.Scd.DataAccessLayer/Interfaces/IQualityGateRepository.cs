using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateRepository
    {
        Task<IEnumerable<BundleDetail>> Check(HistoryContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, long[]> costBlockFilter);

        Task<IEnumerable<BundleDetail>> Check(CostBlockHistory history, IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}
