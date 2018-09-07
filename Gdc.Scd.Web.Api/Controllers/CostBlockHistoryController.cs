using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Api.Controllers
{
    [Produces("application/json")]
    public class CostBlockHistoryController : Controller
    {
        private readonly ICostBlockHistoryService costBlockHistoryService;

        public CostBlockHistoryController(ICostBlockHistoryService costBlockHistoryService)
        {
            this.costBlockHistoryService = costBlockHistoryService;
        }

        [HttpGet]
        public async Task<IEnumerable<ApprovalBundle>> GetApprovalBundles([FromQuery]CostBlockHistoryFilter filter, [FromQuery]CostBlockHistoryState state)
        {
            return await this.costBlockHistoryService.GetApprovalBundles(filter, state);
        }

        [HttpGet]
        public async Task<IEnumerable<Dictionary<string, object>>> GetApproveBundleDetail(
            [FromQuery]long costBlockHistoryId, 
            [FromQuery]long? historyValueId = null, 
            [FromQuery]string costBlockFilter = null)
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
                    [nameof(historyValue.HistoryValueId)] = historyValue.HistoryValueId
                };

                foreach (var dependency in historyValue.InputLevels.Concat(historyValue.Dependencies))
                {
                    dictionary.Add($"{dependency.Key}Id", dependency.Value.Id);
                    dictionary.Add($"{dependency.Key}Name", dependency.Value.Name);
                }

                return dictionary;
            });
        }

        [HttpGet]
        public async Task<IEnumerable<HistoryItem>> GetHistory(
            CostEditorContext context, 
            long editItemId, 
            int? start, 
            int? limit, 
            string sort = null)
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
        public async Task<IActionResult> Approve([FromQuery]long historyId)
        {
            await this.costBlockHistoryService.Approve(historyId);

            return this.Ok();
        }

        [HttpPost]
        public IActionResult Reject([FromQuery]long historyId, [FromQuery]string message)
        {
            this.costBlockHistoryService.Reject(historyId, message);

            return this.Ok();
        }
    }
}
