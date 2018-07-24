using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IEnumerable<CapabilityMatrix>> Allowed()
        {
            var allowed = await capabilityMatrixService.GetAllowedCombinations();
            return allowed.Cast<CapabilityMatrix>();
        }

        [HttpGet]
        public async Task<IEnumerable<CapabilityMatrix>> Denyed()
        {
            var denyed = await capabilityMatrixService.GetDenyedCombinations();
            return denyed.Cast<CapabilityMatrix>();
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
