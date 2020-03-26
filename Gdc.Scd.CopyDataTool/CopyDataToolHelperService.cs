using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.CopyDataTool.Entities;
using Gdc.Scd.Core.Entities;
using System.Linq;

namespace Gdc.Scd.CopyDataTool
{
    public class CopyDataToolHelperService
    {
        private CopyDetailsConfig config;

        private readonly IDomainService<ExchangeRate> exchangeRateService;

        private readonly IDomainService<Country> countryService;

        public CopyDataToolHelperService(
            CopyDetailsConfig config,
            IDomainService<ExchangeRate> exchangeRateService, 
            IDomainService<Country> countryService)
        {
            this.config = config;
            this.exchangeRateService = exchangeRateService;
            this.countryService = countryService;
        }

        public ExcangeRateCalculator GetExcangeRateCalculator()
        {
            ExcangeRateCalculator excangeRateCalculator;

            if (this.config.HasTargetCountry)
            {
                var sourceExchangeRate = GetExchangeRate(this.config.Country);
                var targetExchangeRate = GetExchangeRate(this.config.TargetCountry);

                excangeRateCalculator = new ExcangeRateCalculator(sourceExchangeRate, targetExchangeRate);
            }
            else
            {
                excangeRateCalculator = new ExcangeRateCalculator(1, 1);
            }

            return excangeRateCalculator;

            double GetExchangeRate(string countryName)
            {
                return
                    this.countryService.GetAll()
                                       .Where(country => country.Name == countryName)
                                       .Join(
                                           this.exchangeRateService.GetAll(),
                                           country => country.CurrencyId,
                                           exRate => exRate.Id,
                                           (country, exRate) => exRate.Value)
                                       .First();
            }
        }
    }
}
