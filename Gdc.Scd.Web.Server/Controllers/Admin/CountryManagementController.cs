using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Controllers.Admin
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Admin })]
    public class CountryManagementController : ApiController
    {
        private readonly ICountryAdminService _countryAdminService;

        public CountryManagementController(ICountryAdminService countryAdminService)
        {
            _countryAdminService = countryAdminService;
        }

        [HttpGet]
        public DataInfo<CountryDto> GetAll([FromUri] AdminCountryFilterDto filter, [FromUri] int page = 1, [FromUri] int start = 0, int limit = 50)
        {
            var countries = _countryAdminService.GetAll(page, limit, out int totalCount, filter);

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

        [HttpPost]
        public void ExportToExcel()
        {
            var request = JsonConvert.DeserializeObject<AdminCountryFilterDto>(HttpContext.Current.Request.Form["data"]);
        }
    }
}