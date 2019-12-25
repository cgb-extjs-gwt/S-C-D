using Gdc.Scd.Import.CentralContractGroup;
using Gdc.Scd.Tests.Integration.Import.CentralContractGroup.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.CentralContractGroup
{
    public class CentralContractGroupJobTest : CentralContractGroupJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeCentralContractGroupService fakeContract;

        public CentralContractGroupJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeContract = new FakeCentralContractGroupService();

            log = fakeLogger;
            contract = fakeContract;
        }

        [TestCase]
        public void WhoAmITest()
        {
            Assert.AreEqual("CentralContractGroupJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeContract.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeContract.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeContract.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Central Contract Group Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeContract.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Central Contract Group Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
