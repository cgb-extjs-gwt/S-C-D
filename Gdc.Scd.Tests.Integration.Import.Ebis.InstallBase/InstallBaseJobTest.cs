using Gdc.Scd.Import.Ebis.InstallBase;
using Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase.Fakes;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase
{
    public class InstallBaseJobTest : InstallBaseJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeInstallBaseService fakeInstallBase;

        public InstallBaseJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeInstallBase = new FakeInstallBaseService();

            log = fakeLogger;
            installBaseService = fakeInstallBase;
        }

        [TestCase]
        public void WhoAmITest()
        {
            Assert.AreEqual("InstallBaseJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeInstallBase.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeInstallBase.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeInstallBase.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Ebis: InstallBase Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeInstallBase.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Ebis: InstallBase Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
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
