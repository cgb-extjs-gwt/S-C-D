using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IApprovalService
    {
        Task Approve(long historyId);

        Task<IEnumerable<Bundle>> GetApprovalBundles(CostBlockHistoryFilter filter, CostBlockHistoryState state);

        Task<IEnumerable<Bundle>> GetOwnApprovalBundles(CostBlockHistoryFilter filter, CostBlockHistoryState state);

        Task<IEnumerable<BundleDetailGroup>> GetApproveBundleDetails(CostBlockHistory history, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        Task<IEnumerable<BundleDetailGroup>> GetApproveBundleDetails(long costBlockHistoryId, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        void Reject(long historyId, string message = null);

        Task<QualityGateResult> SendForApproval(long historyId, string qualityGateErrorExplanation = null);
    }
}