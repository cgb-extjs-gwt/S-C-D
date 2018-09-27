using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using System.Threading.Tasks;
using System.Web.Http;

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
        public Task<DataInfo<CapabilityMatrixDto>> Allowed(
                [FromUri]CapabilityMatrixFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 25
            )
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            return capabilityMatrixService.GetAllowedCombinations(filter, start, limit)
                                          .ContinueWith(x => new DataInfo<CapabilityMatrixDto>
                                          {
                                              Items = x.Result.Item1,
                                              Total = x.Result.Item2
                                          });
        }

        [HttpGet]
        public Task<DataInfo<CapabilityMatrixRuleDto>> Denied(
                [FromUri]CapabilityMatrixFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 25
            )
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            return capabilityMatrixService.GetDeniedCombinations(filter, start, limit)
                                          .ContinueWith(x => new DataInfo<CapabilityMatrixRuleDto>
                                          {
                                              Items = x.Result.Item1,
                                              Total = x.Result.Item2
                                          });
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
