using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.Core.Entities;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICountryUserService
    {
        Task<UserCountryDto[]> GetAll(User usr);

        Task<UserCountryDto[]> GetMasterCountries(User usr);

        Task<UserCountryDto[]> GetUserCountries(User usr);

        Task<UserCountryDto[]> GetUserMasterCountries(User usr);

        bool HasCountryAccess(User usr, long countryId);
    }
}
