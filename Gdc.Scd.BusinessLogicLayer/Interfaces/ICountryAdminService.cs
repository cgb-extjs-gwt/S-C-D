﻿using Gdc.Scd.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICountryAdminService
    {
        List<CountryDto> GetAll(int pageNumber, int limit, out int totalCount);
        void Save(IEnumerable<CountryDto> countries);
    }
}