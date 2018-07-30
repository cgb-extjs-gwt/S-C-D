using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;
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
        public async Task<IEnumerable<CapabilityMatrixListDto>> Allowed()
        {
            var allowed = await capabilityMatrixService.GetAllowedCombinations();
            return AsListModel(allowed);
        }

        [HttpGet]
        public async Task<IEnumerable<CapabilityMatrixListDto>> Denied()
        {
            var denyed = await capabilityMatrixService.GetDeniedCombinations();
            return AsListModel(denyed);
        }

        [HttpPost]
        public Task Allow([FromBody]CapabilityMatrixEditDto m)
        {
            return capabilityMatrixService.AllowCombination(m);
        }

        [HttpPost]
        public Task Deny([FromBody]CapabilityMatrixEditDto m)
        {
            return capabilityMatrixService.DenyCombination(m);
        }

        private IEnumerable<CapabilityMatrixListDto> AsListModel(IEnumerable<CapabilityMatrix> items)
        {
            return items.Select(x => new CapabilityMatrixListDto
            {
                Id = x.Id,

                Country = x.Country.GetName(),

                WG = x.Wg.GetName(),
                Availability = x.Availability.GetName(),
                Duration = x.Duration.GetName(),
                ReactionType = x.ReactionType.GetName(),
                ReactionTime = x.ReactionTime.GetName(),
                ServiceLocation = x.ServiceLocation.GetName(),

                IsGlobalPortfolio = x.FujitsuGlobalPortfolio,
                IsMasterPortfolio = x.MasterPortfolio,
                IsCorePortfolio = x.CorePortfolio
            });
        }
    }

    public class CapabilityMatrixListDto
    {
        public long Id { get; set; }

        public string Country { get; set; }

        public string WG { get; set; }
        public string Availability { get; set; }
        public string Duration { get; set; }
        public string ReactionType { get; set; }
        public string ReactionTime { get; set; }
        public string ServiceLocation { get; set; }

        public bool IsGlobalPortfolio { get; set; }
        public bool IsMasterPortfolio { get; set; }
        public bool IsCorePortfolio { get; set; }
    }
}
