using Gdc.Scd.Export.Archive;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    public class ArchiveServiceTest
    {
        [TestCase(TestName = "Archive service should load every table and save every excel to cloud")]
        public void Should_Load_All_Tables_And_Save_To_Cload_Test()
        {
            var logger = new FakeLogger();
            var repo = new FakeArchiveRepository();

            var srv = new ArchiveService(repo, logger);
            srv.Run();

            Assert.True(repo.IsAllLoaded());
            Assert.True(repo.IsAllSaved());
        }
    }
}
