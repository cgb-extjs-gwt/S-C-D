using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
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

        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues([FromQuery]long costBlockHistoryId)
        {
           return await this.costBlockHistoryService.GetHistoryValues(costBlockHistoryId);
        }

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
    }
}
