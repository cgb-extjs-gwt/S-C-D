using Gdc.Scd.Import.Logistics;
using Gdc.Scd.Tests.Integration.Import.Logistics.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Logistics
{
    public class LogisticsJobTest : LogisticsJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeLogisticsImportService fakeImport;

        public LogisticsJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeImport = new FakeLogisticsImportService() { error = null };

            log = fakeLogger;
            logistic = fakeImport;
        }

        [TestCase]
        public void WhoAmI_Should_Return_PorJob_String_Test()
        {
            Assert.AreEqual("LogisticsImportJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeImport.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeImport.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeImport.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Logistics Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeImport.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Logistics Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
