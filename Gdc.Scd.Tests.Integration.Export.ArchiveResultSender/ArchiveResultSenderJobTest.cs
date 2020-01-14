using Gdc.Scd.Tests.Integration.Import.Logistics.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;
using Gdc.Scd.Export.ArchiveResultSenderJob;

namespace Gdc.Scd.Tests.Integration.Export.ArchiveResultSender
{
    public class ArchiveResultSenderJobTest : ArchiveResultSenderJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeArchiveResultService fakeArchive;

        public ArchiveResultSenderJobTest() : base(null, null) { }

        [SetUp]
        public void Setup()
        {
            fakeLogger = new FakeLogger();
            fakeArchive = new FakeArchiveResultService() { error = null };

            log = fakeLogger;
            archive = fakeArchive;
        }

        [TestCase]
        public void WhoAmI_String_Test()
        {
            Assert.AreEqual("ArchiveResultSenderJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeArchive.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeArchive.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeArchive.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Unexpected error occured", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeArchive.error = new Exception("Error here");

            Output();

            Assert.AreEqual("Unexpected error occured", fakeLogger.Message);
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
