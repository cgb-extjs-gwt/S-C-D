using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IApprovalRepository
    {
        Task<int> Approve(CostBlockHistory history);

        Task<IEnumerable<BundleDetail>> GetApproveBundleDetail(CostBlockHistory history, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        Task<IEnumerable<BundleDetail>> GetApproveBundleDetailQualityGate(CostBlockHistory history, bool userCountyGroupCheck, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);
    }
}