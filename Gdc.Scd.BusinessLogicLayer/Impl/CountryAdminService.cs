using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CountryAdminService : ICountryAdminService
    {
        private readonly IRepositorySet _repositorySet;

        private readonly IRepository<Country> _countryRepo;

        public CountryAdminService(IRepositorySet repositorySet,
            IRepository<Country> countryRepo)
        {
            _repositorySet = repositorySet;
            _countryRepo = countryRepo;
        }

        public List<CountryDto> GetAll(int pageNumber, int limit, out int totalCount, AdminCountryFilterDto filter = null)
        {
            var countries = _countryRepo.GetAll();

            if (filter != null)
            {
                countries = countries.WhereIf(filter.Country != null, x => x.Name == filter.Country)
                                     .WhereIf(filter.Group.HasValue, x => x.CountryGroupId == filter.Group.Value)
                                     .WhereIf(filter.Lut != null, x => x.CountryGroup.LUTCode == filter.Lut)
                                     .WhereIf(filter.Digit != null, x => x.CountryGroup.CountryDigit == filter.Digit)
                                     .WhereIf(filter.Iso != null, x => x.ISO3CountryCode == filter.Iso)
                                     .WhereIf(filter.QualityGroup != null, x => x.QualityGateGroup == filter.QualityGroup)
                                     .WhereIf(filter.IsMaster.HasValue, x => x.IsMaster == filter.IsMaster.Value)
                                     .WhereIf(filter.StoreListAndDealer.HasValue, x => x.CanStoreListAndDealerPrices == filter.StoreListAndDealer.Value)
                                     .WhereIf(filter.OverrideTCandTP.HasValue, x => x.CanOverrideTransferCostAndPrice == filter.OverrideTCandTP.Value);
            }

            totalCount = countries.Count();

            countries = countries.OrderBy(c => c.Name).Skip((pageNumber - 1) * limit);

            return countries.Select(c => new CountryDto
            {
                CanOverrideTransferCostAndPrice = c.CanOverrideTransferCostAndPrice,
                CanStoreListAndDealerPrices = c.CanStoreListAndDealerPrices,
                CountryDigit = c.CountryGroup.CountryDigit ?? string.Empty,
                CountryGroup = c.CountryGroup.Name,
                CountryName = c.Name,
                LUTCode = c.CountryGroup.LUTCode ?? string.Empty,
                ISO3Code = c.ISO3CountryCode ?? string.Empty,
                IsMaster = c.IsMaster,
                QualityGroup = c.QualityGateGroup ?? string.Empty,
                Currency = c.Currency.Name,
                CountryId = c.Id
            }).OrderBy(x => x.CountryName).ToList();
        }

        public void Save(IEnumerable<CountryDto> countries)
        {
            var countryDict = countries.ToDictionary(c => c.CountryId);
            var keys = countryDict.Select(d => d.Key).ToList();
            var countriesToUpdate = _countryRepo.GetAll().Where(c => keys.Contains(c.Id));
            foreach (var country in countriesToUpdate)
            {
                country.CanOverrideTransferCostAndPrice = countryDict[country.Id].CanOverrideTransferCostAndPrice;
                country.CanStoreListAndDealerPrices = countryDict[country.Id].CanStoreListAndDealerPrices;
                country.QualityGateGroup = String.IsNullOrEmpty(countryDict[country.Id].QualityGroup?.Trim()) ? null :
                                                                countryDict[country.Id].QualityGroup.Trim();
            }

            _countryRepo.Save(countriesToUpdate);
            _repositorySet.Sync();
        }
    }
}
