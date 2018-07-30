﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

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