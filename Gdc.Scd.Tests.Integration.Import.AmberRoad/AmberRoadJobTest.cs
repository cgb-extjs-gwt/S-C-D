using Gdc.Scd.Import.AmberRoad;
using Gdc.Scd.Tests.Integration.Import.AmberRoad.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.AmberRoad
{
    public class AmberRoadJobTest : AmberRoadJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeAmberRoadService fakeAmber;

        public AmberRoadJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeAmber = new FakeAmberRoadService() { error = null };

            log = fakeLogger;
            amber = fakeAmber;
        }

        [TestCase]
        public void WhoAmI_String_Test()
        {
            Assert.AreEqual("AmberRoadJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeAmber.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeAmber.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeAmber.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Amber Road Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeAmber.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Amber Road Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
