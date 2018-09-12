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
        public DataInfo<CapabilityMatrixDto> Allowed(CapabilityMatrixFilterDto filter, int start = 0, int limit = 25)
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            int total;
            var items = capabilityMatrixService.GetAllowedCombinations(filter, start, limit, out total);

            return new DataInfo<CapabilityMatrixDto> { Items = items, Total = total };
        }

        [HttpGet]
        public DataInfo<CapabilityMatrixRuleDto> Denied(CapabilityMatrixFilterDto filter, int start = 0, int limit = 25)
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            int total;
            var items = capabilityMatrixService.GetDeniedCombinations(filter, start, limit, out total);

            return new DataInfo<CapabilityMatrixRuleDto> { Items = items, Total = total };
        }

        [HttpPost]
        public async Task<object> Allow([FromBody]long[] ids)
        {
            await capabilityMatrixService.AllowCombinations(ids);
            return OkResult();
        }

        [HttpPost]
        public async Task<object> Deny([FromBody]CapabilityMatrixRuleSetDto m)
        {
            await capabilityMatrixService.DenyCombination(m);
            return OkResult();
        }

        private bool isRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }

        object OkResult()
        {
            return new { ok = true };
        }

        private bool isRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }
}
