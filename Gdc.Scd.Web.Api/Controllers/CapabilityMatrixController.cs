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
        public async Task<IEnumerable<CapabilityMatrixListModel>> Allowed()
        {
            var allowed = await capabilityMatrixService.GetAllowedCombinations();
            return AsListModel(allowed);
        }

        [HttpGet]
        public async Task<IEnumerable<CapabilityMatrixListModel>> Denied()
        {
            var denyed = await capabilityMatrixService.GetDeniedCombinations();
            return AsListModel(denyed);
        }

        [HttpPost]
        public Task Allow([FromBody]CapabilityMatrixEditModel m)
        {
            return capabilityMatrixService.AllowCombination();
        }

        [HttpPost]
        public Task Deny([FromBody]CapabilityMatrixEditModel m)
        {
            return capabilityMatrixService.DenyCombination();
        }

        private IEnumerable<CapabilityMatrixListModel> AsListModel(IEnumerable<CapabilityMatrix> items)
        {
            return items.Select(x => new CapabilityMatrixListModel
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

    public class CapabilityMatrixListModel
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

    public class CapabilityMatrixEditModel
    {
        public long? CountryId { get; set; }

        public long[] Wgs { get; set; }
        public long[] Availabilities { get; set; }
        public long[] Durations { get; set; }
        public long[] ReactionTypes { get; set; }
        public long[] ReactionTimes { get; set; }
        public long[] ServiceLocations { get; set; }

        public bool IsGlobalPortfolio { get; set; }
        public bool IsMasterPortfolio { get; set; }
        public bool IsCorePortfolio { get; set; }
    }
}
