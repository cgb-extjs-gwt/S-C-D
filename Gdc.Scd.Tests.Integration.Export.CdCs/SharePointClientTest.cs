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

        [TestCase("sp_loaded_scd_2.0.xlsx")]
        [TestCase("CD_CS_Master File_SCD 2.0_v3.xlsx")]
        public void LoadTest(string fn)
        {
            var data = testing.Load(Config.SpFile);
            ExcelWriterTest.Save(data, fn);
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
