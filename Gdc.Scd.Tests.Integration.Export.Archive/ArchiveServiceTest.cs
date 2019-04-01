using Gdc.Scd.Export.Archive;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    public class ArchiveServiceTest
    {
        [TestCase(TestName = "Run full export integration test")]
        public void Full_Export_Test()
        {
            var logger = new FakeLogger();

            var srv = new ArchiveService(null, logger);
            srv.Run();
        }
    }
}
