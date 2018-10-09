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

        public List<CountryDto> GetAll(int pageNumber, int limit, out int totalCount)
        {
            var countries = _countryRepo.GetAll().OrderBy(c => c.Name).ToList();
            totalCount = countries.Count;

            var result = countries.Skip((pageNumber - 1) * limit).Take(limit)
                                  .Select(c => new CountryDto {
                                      CanOverrideTransferCostAndPrice = c.CanOverrideTransferCostAndPrice,
                                      CanStoreListAndDealerPrices = c.CanStoreListAndDealerPrices,
                                      CountryDigit = c.CountryGroup.CountryDigit ?? String.Empty,
                                      CountryGroup = c.CountryGroup.Name,
                                      CountryName = c.Name,
                                      LUTCode = c.CountryGroup.LUTCode ?? String.Empty,
                                      ISO3Code = c.ISO3CountryCode ?? String.Empty,
                                      IsMaster = c.IsMaster ? "TRUE" : "FALSE",
                                      QualityGroup = c.QualityGateGroup ?? String.Empty,
                                      CountryId = c.Id
                                  });
            return result.ToList();
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
