using Gdc.Scd.Import.Ebis.InstallBase;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase
{
    public class InstallBaseServiceTest 
    {
        private InstallBaseService testing;

        public InstallBaseServiceTest() 
        {
            var kernel = CreateKernel();
            testing = kernel.Get<InstallBaseService>();
        }

        [TestCase]
        public void RunFullImportTest()
        {
            var path = @"C:\Dev\SCD\Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase\bin\Debug\Logs";
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }

            testing.UploadInstallBaseInfo();
        }

        protected StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Scd.Core.Module(),
                new Scd.DataAccessLayer.Module(),
                new Scd.BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
