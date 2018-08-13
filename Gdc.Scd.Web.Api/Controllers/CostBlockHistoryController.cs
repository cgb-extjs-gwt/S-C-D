using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IEnumerable<CostBlockHistoryApprovalDto>> GetDtoHistoriesForApproval([FromQuery]CostBlockHistoryFilter filter)
        {
            return await this.costBlockHistoryService.GetDtoHistoriesForApproval(filter);
        }

        [HttpGet]
        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues([FromQuery]long costBlockHistoryId)
        {
           return await this.costBlockHistoryService.GetHistoryValues(costBlockHistoryId);
        }

        [HttpGet]
        public async Task<IEnumerable<Dictionary<string, object>>> GetHistoryValueTable([FromQuery]long costBlockHistoryId)
        {
            var historyValues = await this.costBlockHistoryService.GetHistoryValues(costBlockHistoryId);

            return historyValues.Select(historyValue =>
            {
                var dictionary = new Dictionary<string, object>
                {
                    ["InputLevelId"] = historyValue.InputLevel.Id,
                    ["InputLevelName"] = historyValue.InputLevel.Name,
                    [nameof(CostBlockValueHistory.Value)] = historyValue.Value,
                };

                foreach (var dependency in historyValue.Dependencies)
                {
                    dictionary.Add($"{dependency.Key}Id", dependency.Value.Id);
                    dictionary.Add($"{dependency.Key}Name", dependency.Value.Name);
                }

                return dictionary;
            });
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
