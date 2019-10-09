using Gdc.Scd.Export.CdCs;
using Gdc.Scd.Tests.Integration.Export.CdCs.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class CdCsJobTest : CdCsJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeCdCsService fakeCdCs;

        public CdCsJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeCdCs = new FakeCdCsService();

            log = fakeLogger;
            cdCs = fakeCdCs;
        }

        [TestCase]
        public void WhoAmI_Should_Return_CdCsJob_String_Test()
        {
            Assert.AreEqual("CdCsJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeCdCs.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeCdCs.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeCdCs.error = new Exception("Error here");

            Output();

            Assert.AreEqual("CD CS Calculation completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeCdCs.error = new Exception("Error here");

            Output();

            Assert.AreEqual("CD CS Calculation completed unsuccessfully. Please find details below.", fakeLogger.Message);
            Assert.AreEqual("Error here", fakeLogger.Exception.Message);
        }

        protected override void Notify(Exception ex, string msg)
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
