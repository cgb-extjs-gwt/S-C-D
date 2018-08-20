using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using System.Net;
using System.Web.Http;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class CostBlockHistoryController : ApiController
    {
        private readonly ICostBlockHistoryService costBlockHistoryService;

        public CostBlockHistoryController(ICostBlockHistoryService costBlockHistoryService)
        {
            this.costBlockHistoryService = costBlockHistoryService;
        }

        [HttpGet]
        public async Task<IEnumerable<CostBlockHistoryApprovalDto>> GetDtoHistoriesForApproval(CostBlockHistoryFilter filter)
        {
            return await this.costBlockHistoryService.GetDtoHistoriesForApproval(filter);
        }

        [HttpGet]
        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(long costBlockHistoryId)
        {
           return await this.costBlockHistoryService.GetHistoryValues(costBlockHistoryId);
        }

        [HttpGet]
        public async Task<IEnumerable<Dictionary<string, object>>> GetHistoryValueTable(long costBlockHistoryId)
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
        public async Task<IHttpActionResult> Approve(long historyId)
        {
            await this.costBlockHistoryService.Approve(historyId);

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Reject([System.Web.Http.FromBody]long historyId, string message)
        {
            this.costBlockHistoryService.Reject(historyId, message);
            return Ok();
        }
    }
}
