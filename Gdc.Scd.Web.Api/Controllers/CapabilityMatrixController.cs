using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    [Produces("application/json")]
    public class CapabilityMatrixController : Controller
    {
        private readonly IDomainService<Country> countryService;

        private readonly IDomainService<Wg> wgService;

        private readonly IDomainService<Availability> availabilityService;

        private readonly IDomainService<Duration> durationService;

        private readonly IDomainService<ReactionType> reactionTypeService;

        private readonly IDomainService<ReactionTime> reactionTimeService;

        private readonly IDomainService<ServiceLocation> serviceLocationService;

        public CapabilityMatrixController()
        {

        }

        [HttpGet]
        public ActionResult Allowed()
        {
            return Ok();
        }

        [HttpGet]
        public ActionResult Denyed()
        {
            return Ok();
        }

        [HttpPost]
        public void Allow()
        {

        }

        [HttpPost]
        public void Deny()
        {

        }

    }
}
