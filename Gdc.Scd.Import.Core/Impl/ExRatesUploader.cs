using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
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
    public class ExRatesUploader : IUploader<ExchangeRateDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<Currency> _repositoryCurrency;
        private readonly IRepository<ExchangeRate> _repositoryExchangeRate;
        private readonly ILogger<LogLevel> _logger;

        public ExRatesUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryCurrency = this._repositorySet.GetRepository<Currency>();
            this._repositoryExchangeRate = this._repositorySet.GetRepository<ExchangeRate>();
            this._logger = logger;
        }

        public IEnumerable<UpdateQueryOption> Upload(IEnumerable<ExchangeRateDto> items, DateTime modifiedDateTime)
        {
            var currencies = _repositoryCurrency.GetAll().ToList();
            var exRates = _repositoryExchangeRate.GetAll().ToList();

            var batchList = new List<ExchangeRate>();

            foreach (var item in items)
            {
                if (String.IsNullOrEmpty(item.CurrencyCode) || !item.ExchangeRate.HasValue)
                    continue;
                var currency = currencies.FirstOrDefault(c => c.Name.Equals(item.CurrencyCode, StringComparison.OrdinalIgnoreCase));
                if (currency == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_CURRENCY, item.CurrencyCode);
                    continue;
                }
                var exchangeRate = exRates.FirstOrDefault(exr => exr.Id == currency.Id);
                if (exchangeRate == null)
                {
                    exchangeRate = new ExchangeRate();
                    exchangeRate.Currency = currency;
                    _logger.Log(LogLevel.Debug, ImportConstants.ADD_EXCHANGE_RATE, item.CurrencyCode);
                }

                exchangeRate.Value = item.ExchangeRate.Value;
                _logger.Log(LogLevel.Debug, ImportConstants.UPDATE_EXCHANGE_RATE, item.CurrencyCode);

                batchList.Add(exchangeRate);
            }

            if (batchList.Any())
            {
                _repositoryExchangeRate.DisableTrigger();
                _repositoryExchangeRate.Save(batchList);
                _repositorySet.Sync();
                _repositoryExchangeRate.EnableTrigger();
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_END, batchList.Count);
            return new List<UpdateQueryOption>();
        } 
    }
}
