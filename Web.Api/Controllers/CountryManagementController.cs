using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Web.Http;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class CountryManagementController : BaseDomainController<Country>
    {
        public CountryManagementController(IDomainService<Country> countryService) : 
            base(countryService)
        {

        }
    }
}