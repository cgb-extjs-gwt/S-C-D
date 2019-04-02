using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Tests.Util;
using Ninject;
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

            Assert.True(repo.GetCostBlocks().Length > 0);
            Assert.True(repo.IsAllLoaded());
            Assert.True(repo.IsAllSaved());
        }

        [TestCase(TestName = "Check countries count test")]
        public void GetCountries_Test()
        {
            var kernel = Module.CreateKernel();
            var repo = new FileArchiveRepository(kernel.Get<IRepositorySet>());
            var countries = repo.GetCountries();
            Assert.Less(0, countries.Length);
            Assert.NotNull(System.Array.Find(countries, x => string.CompareOrdinal(x.Name, "Belgium") == 0));
        }

        [TestCase(TestName = "Full integration test")]
        public void Full_Test()
        {
            var kernel = Module.CreateKernel();
            var repo = new FileArchiveRepository(kernel.Get<IRepositorySet>());
            var logger = kernel.Get<ILogger>();

            var srv = new ArchiveService(repo, logger);
            srv.Run();
        }
    }
}
