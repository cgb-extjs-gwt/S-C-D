using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;
using Gdc.Scd.Core.Entities.QualityGate;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IApprovalService
    {
        Task Approve(long historyId, bool turnOffNotification=false);

        Task<IEnumerable<BundleDto>> GetApprovalBundles(BundleFilter filter);

        Task<IEnumerable<BundleDto>> GetOwnApprovalBundles(BundleFilter filter);

        Task<IEnumerable<BundleDetailGroupDto>> GetApproveBundleDetails(CostBlockHistory history, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        Task<IEnumerable<BundleDetailGroupDto>> GetApproveBundleDetails(long costBlockHistoryId, long? historyValueId = null, IDictionary<string, IEnumerable<object>> costBlockFilter = null);

        Task Reject(long historyId, string message = null, bool turnOffNotification=false);

        Task<QualityGateResult> SendForApproval(long historyId, string qualityGateErrorExplanation = null);
    }
}