using Gdc.Scd.BusinessLogicLayer.Interfaces;
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
        public Task Allowed()
        {
            return capabilityMatrixService.GetAllowedCombinations();
        }

        [HttpGet]
        public Task Denyed()
        {
            return capabilityMatrixService.GetDenyedCombinations();
        }

        [HttpPost]
        public Task Allow()
        {
            return capabilityMatrixService.AllowCombination();
        }

        [HttpPost]
        public Task Deny()
        {
            return capabilityMatrixService.DenyCombination();
        }
    }
}
