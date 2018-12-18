using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
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
                countries = countries.Where(x =>
                    (filter.Country != null ? x.Name == filter.Country : true) &&
                    (filter.Group != null ? x.CountryGroupId == filter.Group : true) &&
                    (filter.Lut != null ? x.CountryGroup.LUTCode == filter.Lut : true) &&
                    (filter.Digit != null ? x.CountryGroup.CountryDigit == filter.Digit : true) &&
                    (filter.Iso != null ? x.ISO3CountryCode == filter.Iso : true) &&
                    (filter.QualityGroup != null ? x.QualityGateGroup == filter.QualityGroup : true) &&
                    (filter.IsMaster != null ? x.IsMaster == filter.IsMaster : true) &&
                    (filter.StoreListAndDealer != null ? x.CanStoreListAndDealerPrices == filter.StoreListAndDealer : true) &&
                    (filter.OverrideTCandTP != null ? x.CanOverrideTransferCostAndPrice == filter.OverrideTCandTP : true)
                );
            }

            totalCount = countries.Count();

            countries = countries.OrderBy(c => c.Name).Skip((pageNumber - 1) * limit);
          
            return countries.Select(c => new CountryDto
            {
                CanOverrideTransferCostAndPrice = c.CanOverrideTransferCostAndPrice,
                CanStoreListAndDealerPrices = c.CanStoreListAndDealerPrices,
                CountryDigit = c.CountryGroup.CountryDigit ?? String.Empty,
                CountryGroup = c.CountryGroup.Name,
                CountryName = c.Name,
                LUTCode = c.CountryGroup.LUTCode ?? String.Empty,
                ISO3Code = c.ISO3CountryCode ?? String.Empty,
                IsMaster = c.IsMaster,
                QualityGroup = c.QualityGateGroup ?? String.Empty,
                CountryId = c.Id
            }).OrderBy(x=>x.CountryName).ToList();
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
