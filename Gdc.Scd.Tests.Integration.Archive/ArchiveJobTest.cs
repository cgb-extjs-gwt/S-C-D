using Gdc.Scd.Archive;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Archive
{
    public class ArchiveJobTest : ArchiveJob
    {
        private string adminMsg;

        private Exception error;

        private FakeArchiveService fakeArchive;

        private FakeLogger fakeLogger;

        [SetUp]
        public void Setup()
        {
            fakeArchive = new FakeArchiveService();
            fakeLogger = new FakeLogger();
            srv = fakeArchive;
            logger = fakeLogger;
        }

        [TestCase(TestName = "Check WhoAmI returns 'ArchiveJob' name of job")]
        public void WhoAmI_Should_Return_ArchiveJob_String_Test()
        {
            Assert.AreEqual("ArchiveJob", WhoAmI());
        }

        [TestCase(TestName = "Check job fail operation result")]
        public void Output_Fail_Result_Test()
        {
            fakeArchive.Fail("Sample error here");

            var res = Output();
            Assert.False(res.IsSuccess);
            Assert.True(res.Result);
        }

        [TestCase(TestName = "Check job send admin message when error")]
        public void Should_Send_Admin_Message_When_Error_Test()
        {
            fakeArchive.Fail("Sample error here");

            this.Output();

            Assert.AreEqual("Archivation completed unsuccessfully. Please find details below.", adminMsg);
            Assert.AreEqual("Sample error here", error.Message);
        }

        [TestCase(TestName = "Check job log error")]
        public void Should_Log_Error_Test()
        {
            fakeArchive.Fail("Big error...");

            this.Output();

            Assert.True(fakeLogger.IsFatal);
            Assert.AreEqual("Archivation completed unsuccessfully. Please find details below.", fakeLogger.Message);
            Assert.AreEqual("Big error...", fakeLogger.Exception.Message);
        }

        [TestCase(TestName = "Check job success operation result")]
        public void Output_Success_Result_Test()
        {
            var res = Output();
            Assert.True(res.IsSuccess);
            Assert.True(res.Result);
        }

        protected override void Init() { } //skip base initialization

        protected override void Notify(string msg, Exception e)
        {
            this.adminMsg = msg;
            this.error = e;
        }
    }
}
