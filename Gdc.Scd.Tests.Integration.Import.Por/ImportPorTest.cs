using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Por;
using Gdc.Scd.Tests.Integration.Import.Por.Testings;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class ImportPorTest : ImportPor
    {
        private StandardKernel kernel;

        public ImportPorTest()
        {
            kernel = Module.CreateKernel();
            //
            log = new ThrowLoggerDecorator(kernel.Get<ILogger>());
            kernel.Rebind<ILogger>().ToConstant(log);
            friese = new FakeFrieseClient(log);
            por = new PorService(kernel);
        }

        [TestCase]
        public void RunTest()
        {
            Run();
        }

        [TestCase]
        public void WhenUploadNewWgCostsShouldBeInit_Test()
        {
            UploadWg();
            UpdateCostBlocks();
            UpdateHwCosts();
        }
    }
}
