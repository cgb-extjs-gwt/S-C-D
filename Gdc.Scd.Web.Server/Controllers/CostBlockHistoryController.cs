using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.BusinessLogicLayer.Entities;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CostBlockHistoryController : System.Web.Http.ApiController
    {
        private readonly ICostBlockHistoryService costBlockHistoryService;

        public CostBlockHistoryController(ICostBlockHistoryService costBlockHistoryService)
        {
            this.costBlockHistoryService = costBlockHistoryService;
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new [] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<ApprovalBundle>> GetApprovalBundles([System.Web.Http.FromUri]CostBlockHistoryFilter filter, [System.Web.Http.FromUri]CostBlockHistoryState state)
        {
            return await this.costBlockHistoryService.GetApprovalBundles(filter, state);
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<Dictionary<string, object>>> GetApproveBundleDetail(
            [System.Web.Http.FromUri]long costBlockHistoryId,
            [System.Web.Http.FromUri]long? historyValueId = null,
            [System.Web.Http.FromUri]string costBlockFilter = null)
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

            var historyValues = await this.costBlockHistoryService.GetApproveBundleDetail(costBlockHistoryId, historyValueId, filterDictionary);

            return historyValues.Select(historyValue =>
            {
                var dictionary = new Dictionary<string, object>
                {
                    [nameof(CostBlockValueHistory.Value)] = historyValue.Value,
                    [nameof(historyValue.HistoryValueId)] = historyValue.HistoryValueId,
                    [nameof(CostBlockValueHistory.IsPeriodError)] = historyValue.IsPeriodError,
                    [nameof(CostBlockValueHistory.IsRegionError)] = historyValue.IsRegionError,
                };

                foreach (var dependency in historyValue.InputLevels.Concat(historyValue.Dependencies))
                {
                    dictionary.Add($"{dependency.Key}Id", dependency.Value.Id);
                    dictionary.Add($"{dependency.Key}Name", dependency.Value.Name);
                }

                return dictionary;
            });
        }

        // TODO: Need return DataInfo object, otherwise live scrol don't work. See BaseDomainController method GetBy.
        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.CostEditor })]
        public async Task<IEnumerable<HistoryItem>> GetHistory(
            [System.Web.Http.FromUri]CostEditorContext context,
            [System.Web.Http.FromUri]long editItemId,
            [System.Web.Http.FromUri]int? start,
            [System.Web.Http.FromUri]int? limit,
            [System.Web.Http.FromUri]string sort = null)
        {
            QueryInfo queryInfo = null;

            if (start.HasValue || limit.HasValue || sort != null)
            {
                queryInfo = new QueryInfo
                {
                    Skip = start,
                    Take = limit
                };

                if (sort != null)
                {
                    queryInfo.Sort = JsonConvert.DeserializeObject<SortInfo[]>(sort).FirstOrDefault();
                }
            }

            return await this.costBlockHistoryService.GetHistory(context, editItemId, queryInfo);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<ActionResult> Approve(long historyId)
        {
            await this.costBlockHistoryService.Approve(historyId);

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public ActionResult Reject(long historyId, string message)
        {
            this.costBlockHistoryService.Reject(historyId, message);

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<QualityGateResultDto> SendForApproval([System.Web.Http.FromUri]long historyId, [System.Web.Http.FromUri]string qualityGateErrorExplanation = null)
        {
            return await this.costBlockHistoryService.SendForApproval(historyId, qualityGateErrorExplanation);
        }
    }
}
