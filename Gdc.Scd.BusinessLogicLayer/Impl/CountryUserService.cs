using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CountryUserService : ICountryUserService
    {
        private readonly IPrincipalProvider principal;

        private readonly IUserService userSrv;

        private GetCountry cmd;

        public CountryUserService(
                IPrincipalProvider principal,
                IUserService userSrv,
                IRepositorySet repo
            )
        {
            this.principal = principal;
            this.userSrv = userSrv;
            this.cmd = new GetCountry(repo);
        }

        public Task<UserCountryDto[]> GetAll()
        {
            return cmd.GetCountries(Login, false);
        }

        public Task<UserCountryDto[]> GetMasterCountries()
        {
            return cmd.GetCountries(Login, true);
        }

        public Task<UserCountryDto[]> GetUserCountries()
        {
            return cmd.GetUserCountries(Login, false);
        }

        public Task<UserCountryDto[]> GetUserMasterCountries()
        {
            return cmd.GetUserCountries(Login, true);
        }

        public bool HasCountryAccess(long countryId)
        {
            var user = userSrv.GetCurrentUser();

            if (user.IsGlobal)
            {
                return true;
            }

            return userSrv.GetCurrentUserCountries().Any(x => x.Id == countryId);
        }

        private string Login
        {
            get
            {
                return principal.GetCurrenctPrincipal().Identity.Name;
            }
        }
    }
}
