using Gdc.Scd.Tests.Integration.Import.ExchangeRatesJob.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.ExchangeRatesJob
{
    public class ExchangeRatesJobTest : Gdc.Scd.Import.ExchangeRatesJob.ExchangeRatesJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeExchangeRateService fakeExchange;

        public ExchangeRatesJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeExchange = new FakeExchangeRateService();

            log = fakeLogger;
            exchange = fakeExchange;
        }

        [TestCase]
        public void WhoAmITest()
        {
            Assert.AreEqual("ExchangeRatesJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeExchange.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeExchange.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeExchange.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Exchange Rates Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeExchange.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Exchange Rates Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
            Assert.AreEqual("Error here", fakeLogger.Exception.Message);
        }

        protected override void Notify(string msg, Exception ex)
        {
            this.notify = new FakeNotify() { Msg = msg, Error = ex };
        }
    }

    class FakeNotify
    {
        public string Msg;
        public Exception Error;
    }
}
