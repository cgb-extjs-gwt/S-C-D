using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class ApprovalController : ApiController
    {
        private readonly IApprovalService approvalService;

        public ApprovalController(IApprovalService approvalService)
        {
            this.approvalService = approvalService;
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval })]
        public async Task<IEnumerable<BundleDto>> GetApprovalBundles([FromUri]CostBlockHistoryState state, [FromUri]BundleFilter filter = null)
        {
            return await this.approvalService.GetApprovalBundles(filter, state);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval })]
        public async Task<IEnumerable<BundleDto>> GetApprovalBundlesByFilter(BundleFilter filter, [FromUri]CostBlockHistoryState state)
        {
            return await this.approvalService.GetApprovalBundles(filter, state);
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<BundleDto>> GetOwnApprovalBundles([FromUri]CostBlockHistoryState state, [FromUri]BundleFilter filter = null)
        {
            return await this.approvalService.GetOwnApprovalBundles(filter, state);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<BundleDto>> GetOwnApprovalBundlesByFilter(BundleFilter filter, [FromUri]CostBlockHistoryState state)
        {
            return await this.approvalService.GetOwnApprovalBundles(filter, state);
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<BundleDetailGroupDto>> GetApproveBundleDetail(
            [FromUri]long costBlockHistoryId,
            [FromUri]long? historyValueId = null,
            [FromUri]string costBlockFilter = null)
        {
            Dictionary<string, IEnumerable<object>> filterDictionary = null;

            if (!string.IsNullOrEmpty(costBlockFilter))
            {
                filterDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, long[]>>(costBlockFilter)
                               .ToDictionary(
                                    keyValue => keyValue.Key,
                                    keyValue => keyValue.Value.Cast<object>());
            }

            return await this.approvalService.GetApproveBundleDetails(costBlockHistoryId, historyValueId, filterDictionary);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IHttpActionResult> Approve(long historyId)
        {
            await this.approvalService.Approve(historyId);

            return Ok();
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public IHttpActionResult Reject(long historyId, string message)
        {
            this.approvalService.Reject(historyId, message);

            return Ok();
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<QualityGateResult> SendForApproval([FromUri]long historyId, [FromUri]string qualityGateErrorExplanation = null)
        {
            return await this.approvalService.SendForApproval(historyId, qualityGateErrorExplanation);
        }
    }
}