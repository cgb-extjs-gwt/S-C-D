using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers.Dict
{
    public class CountryController : ApiController
    {
        private readonly IDomainService<Country> domainService;

        private readonly IUserService userSrv;

        public CountryController(
                IDomainService<Country> domainService,
                IUserService userSrv
            )
        {
            this.domainService = domainService;
            this.userSrv = userSrv;
        }

        [HttpGet]
        public Task<CountryDto2[]> GetAll()
        {
            var query = domainService.GetAll().Where(x => x.IsMaster);
            return AsDto(query);
        }

        [HttpGet]
        public Task<CountryDto2[]> Usr()
        {
            if (userSrv.GetCurrentUser().IsGlobal)
            {
                return GetAll();
            }
            else
            {
                var query = userSrv.GetCurrentUserCountries().Where(x => x.IsMaster);
                return AsDto(query);
            }
        }

        [HttpGet]
        public Task<string[]> Iso()
        {
            return domainService.GetAll()
                                .Select(x => x.ISO3CountryCode)
                                .Distinct()
                                .GetAsync();
        }

        private Task<CountryDto2[]> AsDto(IQueryable<Country> query)
        {
            return query.Select(x => new CountryDto2
            {
                Id = x.Id,
                Name = x.Name,
                CanOverrideTransferCostAndPrice = x.CanOverrideTransferCostAndPrice,
                CanStoreListAndDealerPrices = x.CanStoreListAndDealerPrices,
                IsMaster = x.IsMaster,
            })
                        .GetAsync();
        }

        public class CountryDto2
        {
            public bool CanOverrideTransferCostAndPrice { get; set; }
            public bool CanStoreListAndDealerPrices { get; set; }
            public string Name { get; set; }
            public bool IsMaster { get; set; }
            public long Id { get; set; }
        }
    }
}
