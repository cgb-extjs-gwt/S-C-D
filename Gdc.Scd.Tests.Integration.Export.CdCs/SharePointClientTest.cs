using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.CdCsJob;
using Gdc.Scd.Tests.Integration.Export.CdCs.Testings;
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
            var cfg = Config.SpFile;
            var data = testing.Load(cfg);
            ExcelWriterTest.Save(data, cfg.FileName);
        }

        [Ignore("for one time test only")]
        [TestCase]
        public void SendTest()
        {
            var data = FileSharePointClient.GetDoc();
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
