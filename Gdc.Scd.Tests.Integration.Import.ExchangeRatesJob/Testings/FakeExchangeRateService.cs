using Gdc.Scd.Import.ExchangeRatesJob;
using System;

namespace Gdc.Scd.Tests.Integration.Import.ExchangeRatesJob.Testings
{
    class FakeExchangeRateService: ExchangeRateService
    {
        public Exception error;

        public FakeExchangeRateService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
