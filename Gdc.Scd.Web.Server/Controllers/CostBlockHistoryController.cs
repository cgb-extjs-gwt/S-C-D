using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.BusinessLogicLayer.Entities;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CostBlockHistoryController : ApiController
    {
        private readonly ICostBlockHistoryService costBlockHistoryService;

        public CostBlockHistoryController(ICostBlockHistoryService costBlockHistoryService)
        {
            this.costBlockHistoryService = costBlockHistoryService;
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new [] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<ApprovalBundle>> GetApprovalBundles([FromUri]CostBlockHistoryFilter filter, [FromUri]CostBlockHistoryState state)
        {
            return await this.costBlockHistoryService.GetApprovalBundles(filter, state);
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IEnumerable<Dictionary<string, object>>> GetApproveBundleDetail(
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
        public async Task<IEnumerable<HistoryItem>> GetCostEditorHistory(
            [FromUri]CostEditorContext context,
            [FromUri]long editItemId,
            [FromUri]int? start,
            [FromUri]int? limit,
            [FromUri]string sort = null)
        {
            var queryInfo = this.GetQueryInfo(start, limit, sort);

            return await this.costBlockHistoryService.GetHistoryItems(context, editItemId, queryInfo);
        }

        // TODO: Need return DataInfo object, otherwise live scrol don't work. See BaseDomainController method GetBy.
        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.TableView })]
        public async Task<IEnumerable<HistoryItem>> GetTableViewHistory(
            [FromUri]CostElementIdentifier costElementId,
            [FromUri]IDictionary<string, long> coordinates,
            [FromUri]int? start,
            [FromUri]int? limit,
            [FromUri]string sort = null)
        {
            var queryInfo = this.GetQueryInfo(start, limit, sort);

            return await this.costBlockHistoryService.GetHistoryItems(costElementId, coordinates, queryInfo);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<IHttpActionResult> Approve(long historyId)
        {
            await this.costBlockHistoryService.Approve(historyId);

            return Ok();
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public IHttpActionResult Reject(long historyId, string message)
        {
            this.costBlockHistoryService.Reject(historyId, message);

            return Ok();
        }

        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.Approval, PermissionConstants.OwnApproval })]
        public async Task<QualityGateResultDto> SendForApproval([FromUri]long historyId, [FromUri]string qualityGateErrorExplanation = null)
        {
            return await this.costBlockHistoryService.SendForApproval(historyId, qualityGateErrorExplanation);
        }

        private QueryInfo GetQueryInfo(int? start, int? limit, string sort)
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

            return queryInfo;
        }
    }
}
