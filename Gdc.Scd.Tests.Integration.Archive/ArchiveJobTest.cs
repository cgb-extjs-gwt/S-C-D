using Gdc.Scd.Archive;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Archive
{
    public class ArchiveJobTest: ArchiveJob
    {
        private string adminMsg;
        private Exception operException;

        private FakeArchiveService fakeArchive;

        public ArchiveJobTest()
        {
            fakeArchive = new FakeArchiveService();
            srv = fakeArchive;
        }

        [TestCase(TestName = "Check WhoAmI returns 'ArchiveJob' name of job")]
        public void WhoAmI_Should_Return_ArchiveJob_String_Test()
        {
            Assert.AreEqual("ArchiveJob", WhoAmI());
        }

        [TestCase(TestName = "Check job operation result when error")]
        public void Output_Should_Return_False_When_Error_Test()
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
            Assert.AreEqual("Sample error here", operException.Message);
        }

        protected override void Notify(string msg, Exception e)
        {
            this.adminMsg = msg;
            this.operException = e;
        }
    }
}
