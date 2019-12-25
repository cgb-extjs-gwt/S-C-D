using Gdc.Scd.Import.Ebis.MCiW;
using Gdc.Scd.Tests.Integration.Import.Ebis.MCiW.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.MCiW
{
    public class MCiWJobTest : MCiWJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeMaterialCostService fakeMciw;

        public MCiWJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeMciw = new FakeMaterialCostService();

            log = fakeLogger;
            mciw = fakeMciw;
        }

        [TestCase]
        public void WhoAmITest()
        {
            Assert.AreEqual("MCiWJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeMciw.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeMciw.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeMciw.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Ebis: MaterialCost Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeMciw.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Ebis: MaterialCost Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
