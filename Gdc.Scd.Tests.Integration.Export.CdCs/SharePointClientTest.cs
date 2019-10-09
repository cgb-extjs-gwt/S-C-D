using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.CdCs;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class SharePointClientTest
    {
        private SharePointClient testing;

        public SharePointClientTest()
        {
            testing = new SharePointClient(Config.NetworkCredential);
        }

        [TestCase]
        public void LoadTest()
        {
            var data = testing.Load(Config.SpFile);
            ExcelWriterTest.Save(data, "sp_loaded.xlsm");
        }

        [Ignore("for one time test only")]
        [TestCase]
        public void SendTest()
        {
            var data = ExcelWriterTest.GetDoc();
            var cfg = new CdCsConfiguration
            {
                FileFolderUrl = "/02/sites/p/ServiceCostDatabase/CGER/CD_CS_CalculationTool",
                FileWebUrl = "http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CGER",
                Country = new Country
                {
                    Name = "For test"
                }
            };
            testing.Send(data, cfg);
        }
    }
}
