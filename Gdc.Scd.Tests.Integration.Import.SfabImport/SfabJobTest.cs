using Gdc.Scd.Import.SfabImport;
using Gdc.Scd.Tests.Integration.Import.SfabImport.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.Afr
{
    public class SfabJobTest : SfabJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeSFabService fakeAfr;

        public SfabJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeAfr = new FakeSFabService();

            log = fakeLogger;
            sfab = fakeAfr;
        }

        [TestCase]
        public void WhoAmITest()
        {
            Assert.AreEqual("SfabImportJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeAfr.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeAfr.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeAfr.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Software & Solution Sfab Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeAfr.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Software & Solution Sfab Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
