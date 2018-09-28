using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CapabilityMatrixController : ApiController
    {
        private readonly ICapabilityMatrixService capabilityMatrixService;

        public CapabilityMatrixController(
                ICapabilityMatrixService capabilityMatrixService
            )
        {
            this.capabilityMatrixService = capabilityMatrixService;
        }

        [HttpGet]
        public DataInfo<CapabilityMatrixDto> Allowed([FromUri]CapabilityMatrixFilterDto filter, [FromUri]int start = 0, [FromUri]int limit = 25)
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
        public DataInfo<CapabilityMatrixRuleDto> Denied([FromUri]CapabilityMatrixFilterDto filter, [FromUri]int start = 0, [FromUri]int limit = 25)
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
    }
}
