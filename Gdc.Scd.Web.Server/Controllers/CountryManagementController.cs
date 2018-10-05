using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Web.Mvc;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Web.Server.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CountryManagementController : System.Web.Http.ApiController
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
        public void SaveAll([System.Web.Http.FromBody]IEnumerable<CountryDto> records)
        {
            _countryAdminService.Save(records);
        }
    }
}