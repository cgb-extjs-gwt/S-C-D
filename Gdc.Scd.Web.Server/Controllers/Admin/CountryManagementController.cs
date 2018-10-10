using System.Collections.Generic;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    public class CountryManagementController : ApiController
    {
        private readonly ICountryAdminService _countryAdminService;

        public CountryManagementController(ICountryAdminService countryAdminService)
        {
            _countryAdminService = countryAdminService;
        }

        [HttpGet]
        public DataInfo<CountryDto> GetAll(int page = 1, int start = 0, int limit = 50)
        {
            int totalCount;
            var countries = _countryAdminService.GetAll(page, limit, out totalCount);

            var model = new DataInfo<CountryDto>()
            {
                Items = countries,
                Total = totalCount
            };

            return model;
        }

        [HttpPost]
        public void SaveAll([FromBody]IEnumerable<CountryDto> records)
        {
            _countryAdminService.Save(records);
        }
    }
}