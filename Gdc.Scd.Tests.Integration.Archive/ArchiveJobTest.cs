using Gdc.Scd.Archive;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Archive
{
    public class ArchiveJobTest
    {
        [TestCase(TestName = "Check WhoAmI returns 'ArchiveJob' name of job")]
        public void WhoAmI_Should_Return_ArchiveJob_String_Test()
        {
            Assert.AreEqual("ArchiveJob", new ArchiveJob().WhoAmI());
        }
    }
}
