using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class CapabilityMatrixController : System.Web.Http.ApiController
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
            var items = capabilityMatrixService.GetAllowedCombinations(filter, 0, 25, out total);  //TODO: remove hard coded values

            return new DataInfo<CapabilityMatrixDto> { Items = items, Total = total };
        }

        [HttpGet]
        public DataInfo<CapabilityMatrixRuleDto> Denied(CapabilityMatrixFilterDto filter)
        {
            int total;
            var items = capabilityMatrixService.GetDeniedCombinations(filter, 0, 25, out total); //TODO: remove hard coded values

            return new DataInfo<CapabilityMatrixRuleDto> { Items = items, Total = total };
        }

        [HttpPost]
        public Task Allow([System.Web.Http.FromBody]long[] ids)
        {
            return capabilityMatrixService.AllowCombinations(ids);
        }

        [HttpPost]
        public Task Deny([System.Web.Http.FromBody]CapabilityMatrixRuleSetDto m)
        {
            return capabilityMatrixService.DenyCombination(m);
        }
    }
}
