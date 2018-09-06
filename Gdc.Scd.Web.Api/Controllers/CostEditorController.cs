using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    [Produces("application/json")]
    public class CostEditorController : Controller
    {
        private readonly ICostEditorService costEditorService;

        private readonly IDomainMetaSevice domainMetaSevice;

        private readonly DomainMeta meta;

        public CostEditorController(
            ICostEditorService costEditorService, 
            IDomainMetaSevice domainMetaSevice,
            DomainMeta meta)
        {
            this.costEditorService = costEditorService;
            this.domainMetaSevice = domainMetaSevice;
            this.meta = meta;
        }

        [HttpGet]
        public DomainMeta GetCostEditorData()
        {
            return this.meta;
        }

        public async Task<CostElementData> GetCostElementData(CostEditorContext context)
        {
            return await this.costEditorService.GetCostElementData(context);
        }

        [HttpGet]
        public async Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context)
        {
            return await this.costEditorService.GetInputLevelFilterItems(context);
        }

        [HttpGet]
        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            return await this.costEditorService.GetEditItems(context);
        }

        [HttpPost]
        public async Task<QualityGateResultDto> UpdateValues([FromBody]IEnumerable<EditItem> editItems, [FromQuery]CostEditorContext context, [FromQuery]ApprovalOption approvalOption)
        {
            var qualityGateResult = await this.costEditorService.UpdateValues(editItems, context, approvalOption);
            var errors = new List<IDictionary<string, object>>();

            if (qualityGateResult.Errors != null)
            {
                foreach (var error in qualityGateResult.Errors)
                {
                    var errorDictionary = new Dictionary<string, object>
                    {
                        ["WarrantyGroupId"] = error.LastInputLevel.Id,
                        ["WarrantyGroupName"] = error.LastInputLevel.Name,
                        [nameof(error.IsPeriodError)] = error.IsPeriodError,
                        [nameof(error.IsRegionError)] = error.IsRegionError
                    };

                    foreach (var dependency in error.Dependencies)
                    {
                        errorDictionary.Add($"{dependency.Key}Id", dependency.Value.Id);
                        errorDictionary.Add($"{dependency.Key}Name", dependency.Value.Name);
                    }

                    errors.Add(errorDictionary);
                }
            }

            return new QualityGateResultDto
            {
                HasErrors = qualityGateResult.HasErrors,
                Errors = errors
            };
        }
    }
}
