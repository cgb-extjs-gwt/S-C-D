using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CountryAdminService : ICountryAdminService
    {
        private readonly IRepositorySet _repositorySet;

        private readonly IRepository<Country> _countryRepo;

        private readonly IRepository<Currency> _currencyRepo;

        public CountryAdminService(
                IRepositorySet repositorySet,
                IRepository<Country> countryRepo,
                IRepository<Currency> currencyRepo
            )
        {
            _repositorySet = repositorySet;
            _countryRepo = countryRepo;
            _currencyRepo = currencyRepo;
        }

        public List<CountryDto> GetAll(int pageNumber, int limit, out int totalCount, AdminCountryFilterDto filter = null)
        {
            var countries = _countryRepo.GetAll();

            if (filter != null)
            {
                countries = countries.WhereIf(filter.Country != null, x => x.Name == filter.Country)
                                     .WhereIf(filter.Group.HasValue, x => x.CountryGroupId == filter.Group.Value)
                                     .WhereIf(filter.Region.HasValue, x => x.RegionId == filter.Region.Value)
                                     .WhereIf(filter.Lut != null, x => x.CountryGroup.LUTCode == filter.Lut)
                                     .WhereIf(filter.Digit != null, x => x.CountryGroup.CountryDigit == filter.Digit)
                                     .WhereIf(filter.Iso != null, x => x.ISO3CountryCode == filter.Iso)
                                     .WhereIf(filter.QualityGroup != null, x => x.QualityGateGroup == filter.QualityGroup)
                                     .WhereIf(filter.IsMaster.HasValue, x => x.IsMaster == filter.IsMaster.Value)
                                     .WhereIf(filter.StoreListAndDealer.HasValue, x => x.CanStoreListAndDealerPrices == filter.StoreListAndDealer.Value)
                                     .WhereIf(filter.OverrideTCandTP.HasValue, x => x.CanOverrideTransferCostAndPrice == filter.OverrideTCandTP.Value)
                                     .WhereIf(filter.Override2ndLevelSupportLocal.HasValue, x => x.CanOverride2ndLevelSupportLocal == filter.Override2ndLevelSupportLocal.Value);
            }

            totalCount = countries.Count();

            countries = countries.OrderBy(c => c.Name).Skip((pageNumber - 1) * limit);

            return countries.Select(c => new CountryDto
            {
                CanOverrideTransferCostAndPrice = c.CanOverrideTransferCostAndPrice,
                CanOverride2ndLevelSupportLocal = c.CanOverride2ndLevelSupportLocal,
                CanStoreListAndDealerPrices = c.CanStoreListAndDealerPrices,
                CountryDigit = c.CountryGroup.CountryDigit ?? string.Empty,
                CountryGroup = c.CountryGroup.Name,
                CountryName = c.Name,
                LUTCode = c.CountryGroup.LUTCode ?? string.Empty,
                ISO3Code = c.ISO3CountryCode ?? string.Empty,
                IsMaster = c.IsMaster,
                QualityGroup = c.QualityGateGroup ?? string.Empty,
                Currency = c.Currency.Name,
                CountryId = c.Id,
                Region = c.Region.Name
            }).OrderBy(x => x.CountryName).ToList();
        }

        public void Save(IEnumerable<CountryDto> countries)
        {
            var countryDict = countries.ToDictionary(c => c.CountryId);
            var keys = countryDict.Select(d => d.Key).ToList();
            var countriesToUpdate = _countryRepo.GetAll().Where(c => keys.Contains(c.Id));
            var currencies = SelectCurrencies(countries);

            foreach (var country in countriesToUpdate)
            {
                var dto = countryDict[country.Id];
                //
                country.CanOverrideTransferCostAndPrice = dto.CanOverrideTransferCostAndPrice;
                country.CanStoreListAndDealerPrices = dto.CanStoreListAndDealerPrices;
                country.CanOverride2ndLevelSupportLocal = dto.CanOverride2ndLevelSupportLocal;
                country.QualityGateGroup = string.IsNullOrEmpty(dto.QualityGroup?.Trim()) ? null : dto.QualityGroup.Trim();
                country.CurrencyId = currencies[dto.Currency].Id;
            }

            _countryRepo.Save(countriesToUpdate);
            _repositorySet.Sync();
        }

        private Dictionary<string, Currency> SelectCurrencies(IEnumerable<CountryDto> countries)
        {
            var keys = countries.Select(x => x.Currency);
            return _currencyRepo.GetAll().Where(c => keys.Contains(c.Name)).ToDictionary(x => x.Name);
        }
    }
}
