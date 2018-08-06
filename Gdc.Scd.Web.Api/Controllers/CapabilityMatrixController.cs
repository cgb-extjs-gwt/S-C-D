using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gdc.Scd.Web.Api.Controllers
{
    [Produces("application/json")]
    public class CapabilityMatrixController : Controller
    {
        private readonly ICapabilityMatrixService capabilityMatrixService;

        public CapabilityMatrixController(
                ICapabilityMatrixService capabilityMatrixService
            )
        {
            this.capabilityMatrixService = capabilityMatrixService;
        }

        [HttpGet]
        public DataInfo<CapabilityMatrixDto> Allowed(CapabilityMatrixFilterDto filter)
        {
            int total;
            var items = capabilityMatrixService.GetAllowedCombinations(filter, 0, 25, out total);

            return new DataInfo<CapabilityMatrixDto> { Items = items, Total = total };
        }

        [HttpGet]
        public DataInfo<CapabilityMatrixRuleDto> Denied(CapabilityMatrixFilterDto filter)
        {
            int total;
            var items= capabilityMatrixService.GetDeniedCombinations(filter, 0, 25, out total);

            return new DataInfo<CapabilityMatrixRuleDto> { Items = items, Total = total };
        }

        [HttpPost]
        public Task Allow([FromBody]long[] ids)
        {
            return capabilityMatrixService.AllowCombinations(ids);
        }

        [HttpPost]
        public Task Deny([FromBody]CapabilityMatrixRuleSetDto m)
        {
            return capabilityMatrixService.DenyCombination(m);
        }
    }
}
