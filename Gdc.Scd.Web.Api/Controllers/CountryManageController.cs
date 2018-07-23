using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class CountryManageController : BaseDomainController<Country>
    {
        public CountryManageController(IDomainService<Country> countryService) : 
            base(countryService)
        {

        }
    }
}