using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CountryUserService : ICountryUserService
    {
        private const string GET_CNT_FN = "dbo.GetCountries";

        private const string GET_USER_CNT_FN = "dbo.GetUserCountries";

        private readonly IRepository<Country> cntRepo;

        private readonly IUserService userSrv;

        public CountryUserService(
                IUserService userSrv,
                IRepository<Country> cntRepo,
                IRepositorySet repo
            )
        {
            this.userSrv = userSrv;
            this.cntRepo = cntRepo;
        }

        public Task<UserCountryDto[]> GetAll(User usr)
        {
            return Select(GET_CNT_FN, usr.Login, false);
        }

        public Task<UserCountryDto[]> GetMasterCountries(User usr)
        {
            return Select(GET_CNT_FN, usr.Login, true);
        }

        public Task<UserCountryDto[]> GetUserCountries(User usr)
        {
            return Select(GET_USER_CNT_FN, usr.Login, false);
        }

        public Task<UserCountryDto[]> GetUserMasterCountries(User usr)
        {
            return Select(GET_USER_CNT_FN, usr.Login, true);
        }

        public Task<UserCountryDto[]> Select(string fn, string login, bool master)
        {
            var query = cntRepo.GetAll().FromSql(string.Concat("SELECT * FROM ", fn, "({0})"), login);

            if (master)
            {
                query = query.Where(x => x.IsMaster);
            }

            return query.OrderBy(x => x.Name)
                        .Select(x => new UserCountryDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            IsMaster = x.IsMaster,
                            CanOverrideTransferCostAndPrice = x.CanOverrideTransferCostAndPrice,
                            CanStoreListAndDealerPrices = x.CanStoreListAndDealerPrices,
                            ISO3Code = x.ISO3CountryCode
                        })
                        .GetAsync();
        }

        public bool HasCountryAccess(User usr, long countryId)
        {
            if (usr.IsGlobal)
            {
                return true;
            }

            //TODO: remove user service, use user parameter
            return userSrv.GetCurrentUserCountries().Any(x => x.Id == countryId);
        }
    }
}
