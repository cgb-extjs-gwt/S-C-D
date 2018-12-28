using Gdc.Scd.BusinessLogicLayer.Dto;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICountryUserService
    {
        Task<UserCountryDto[]> GetAll();

        Task<UserCountryDto[]> GetMasterCountries();

        Task<UserCountryDto[]> GetUserCountries();

        Task<UserCountryDto[]> GetUserMasterCountries();

        bool HasCountryAccess(long countryId);
    }
}
