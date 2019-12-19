using Gdc.Scd.Import.Ebis.Afr;
using Gdc.Scd.Tests.Integration.Import.Ebis.Afr.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.Afr
{
    public class AfrJobTest : AfrJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeAfrService fakeAfr;

        public AfrJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeAfr = new FakeAfrService();

            log = fakeLogger;
            afr = fakeAfr;
        }

        [TestCase]
        public void WhoAmITest()
        {
            Assert.AreEqual("AfrJob", WhoAmI());
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

            Assert.AreEqual("Ebis: AFR Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeAfr.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Ebis: AFR Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
