using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.CdCs;
using Gdc.Scd.Tests.Integration.Export.CdCs.Testings;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class CdCsServiceTest
    {
        private IKernel kernel;

        private IRepositorySet repo;

        private CdCsService testing;

        public CdCsServiceTest()
        {
            kernel = Gdc.Scd.Export.CdCs.Module.CreateKernel();
            repo = kernel.Get<IRepositorySet>();

            testing = new CdCsService(repo, new FileSharePointClient(), new FakeLogger());
        }

        [TestCase]
        public void RunTest()
        {
            testing.Run();
        }
    }
}
