using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto.AvailabilityFee;
using Gdc.Scd.Web.Server.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    public class AvailabilityFeeAdminController : ApiController
    {
        private readonly IAvailabilityFeeAdminService availabilityFeeAdminService;

        public AvailabilityFeeAdminController(IAvailabilityFeeAdminService service)
        {
            availabilityFeeAdminService = service;
        }

        [HttpGet]
        public DataInfo<AdminAvailabilityFeeViewDto> GetAll(int page = 1, int start = 0, int limit = 25)
        {
            int totalCount;
            var allAvailabilityFeeCombinations = availabilityFeeAdminService.GetAllCombinations(page, limit, out totalCount);
            var mappedCombinations = allAvailabilityFeeCombinations.Select(af => new AdminAvailabilityFeeViewDto
            {
                CountryId = af.CountryId,
                CountryName = af.CountryName,
                ReactionTimeId = af.ReactionTimeId,
                ReactionTimeName = af.ReactionTimeName,
                ReactionTypeId = af.ReactionTypeId,
                ReactionTypeName = af.ReactionTypeName,
                ServiceLocatorId = af.ServiceLocatorId,
                ServiceLocatorName = af.ServiceLocatorName,
                IsApplicable = af.Id.HasValue,
                InnerId = af.Id ?? 0
            }).ToList();

            var model = new DataInfo<AdminAvailabilityFeeViewDto>()
            {
                Items = mappedCombinations,
                Total = totalCount
            };

            return model;
        }

        [HttpPost]
        public void SaveAll([FromBody]IEnumerable<AdminAvailabilityFeeViewDto> records)
        {
            availabilityFeeAdminService.SaveCombinations(records);
        }
    }
}