using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        public IEnumerable<CapabilityMatrixDto> Allowed(CapabilityMatrixFilterDto filter)
        {
            return capabilityMatrixService.GetAllowedCombinations(filter);
        }

        [HttpGet]
        public IEnumerable<CapabilityMatrixDto> Denied(CapabilityMatrixFilterDto filter)
        {
            return capabilityMatrixService.GetDeniedCombinations(filter);
        }

        [HttpPost]
        public Task Allow([FromBody]CapabilityMatrixEditDto m)
        {
            return capabilityMatrixService.AllowCombination(m);
        }

        [HttpPost]
        public Task AllowById([FromBody]long[] ids)
        {
            return capabilityMatrixService.AllowCombinations(ids);
        }

        [HttpPost]
        public Task Deny([FromBody]CapabilityMatrixEditDto m)
        {
            return capabilityMatrixService.DenyCombination(m);
        }
    }
}
