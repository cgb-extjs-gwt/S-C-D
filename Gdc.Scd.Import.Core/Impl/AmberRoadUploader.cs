using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class AmberRoadUploader : IUploader<TaxAndDutiesDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<TaxAndDutiesEntity> _repositoryTaxAndDuties;
        private readonly IRepository<Country> _repositoryCountry;
        private readonly ILogger<LogLevel> _logger;

        public AmberRoadUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryTaxAndDuties = this._repositorySet.GetRepository<TaxAndDutiesEntity>();
            this._repositoryCountry = this._repositorySet.GetRepository<Country>();
            this._logger = logger;
        }

        public int Upload(IEnumerable<TaxAndDutiesDto> items, DateTime modifiedDateTime)
        {
            var dbItemsTaxAndDuties = this._repositoryTaxAndDuties.GetAll().ToList();
            var dbItemsCountries = this._repositoryCountry.GetAll().Where(c => c.IsMaster).ToList();
            var result = 0;

            foreach (var item in items)
            {
                Country country = null;
                if (String.IsNullOrEmpty(item.Country) && String.IsNullOrEmpty(item.ISO3Code))
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.EMPTY_COUNTRY);
                    continue;
                }

                if (String.IsNullOrEmpty(item.Country))
                    country = dbItemsCountries.FirstOrDefault(c => c.ISO3CountryCode.Equals(item.ISO3Code, StringComparison.OrdinalIgnoreCase));

                else if (String.IsNullOrEmpty(item.ISO3Code))
                    country = dbItemsCountries.FirstOrDefault(c => c.Name.Equals(item.Country,
                    StringComparison.OrdinalIgnoreCase));

                else
                {
                    country = dbItemsCountries.FirstOrDefault(c => c.Name.Equals(item.Country,
                        StringComparison.OrdinalIgnoreCase) || c.ISO3CountryCode.Equals(item.ISO3Code, StringComparison.OrdinalIgnoreCase));
                }

                if (country == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_COUNTRY, item.Country, item.ISO3Code);
                    continue;
                }

                var taxAndDutyEntities = dbItemsTaxAndDuties.Where(entity => entity.CountryId == country.Id).ToList();

                if (taxAndDutyEntities.Any())
                {
                    foreach (var entity in taxAndDutyEntities)
                    {
                        entity.TaxAndDuties = item.AverageSumDutiesAndTaxes;
                        entity.ModifiedDateTime = modifiedDateTime;
                        entity.DeactivatedDateTime = null;
                        this._repositoryTaxAndDuties.Save(entity);
                        result++;
                    }
                }

                else
                {
                    var entity = new TaxAndDutiesEntity();
                    entity.CountryId = country.Id;
                    entity.ModifiedDateTime = modifiedDateTime;
                    entity.CreatedDateTime = modifiedDateTime;
                    entity.DeactivatedDateTime = null;
                    entity.TaxAndDuties = item.AverageSumDutiesAndTaxes;
                    this._repositoryTaxAndDuties.Save(entity);
                    result ++;
                }
            }

            this._repositorySet.Sync();
            return result;
        }

        public int Deactivate(DateTime modifiedDateTime)
        {
            var dbItemsCountries = this._repositoryCountry.GetAll().Where(c => c.IsMaster).Select(c => c.Id).ToList();
            var dbItemsTaxAndDuties = this._repositoryTaxAndDuties.GetAll().ToList();

            var itemsToDeactivate = dbItemsTaxAndDuties.Where(entity => !dbItemsCountries.Contains(entity.CountryId)).ToList();
            foreach (var item in itemsToDeactivate)
            {
                _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATING_ENTITY, nameof(TaxAndDutiesEntity), item.CountryId);
                item.DeactivatedDateTime = modifiedDateTime;
                item.ModifiedDateTime = modifiedDateTime;
            }

            if (itemsToDeactivate.Any())
            {
                this._repositoryTaxAndDuties.Save(itemsToDeactivate);
                this._repositorySet.Sync();
            }

            return itemsToDeactivate.Count;
        }


    }
}
