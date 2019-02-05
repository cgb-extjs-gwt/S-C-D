using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.Core.Dto;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICountryAdminService
    {
        List<CountryDto> GetAll(int pageNumber, int limit, out int totalCount, AdminCountryFilterDto filter = null);
        void Save(IEnumerable<CountryDto> countries);
    }
}
