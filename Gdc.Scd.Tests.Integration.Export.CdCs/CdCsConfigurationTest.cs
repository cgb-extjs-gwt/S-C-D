using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.CdCs.Helpers;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class CdCsConfigurationTest
    {
        [TestCase("cnt here")]
        [TestCase("404")]
        [TestCase("No name")]
        public void GetCountryNameTest(string expected)
        {
            var c = new CdCsConfiguration()
            {
                Country = new Country
                {
                    Name = expected
                }
            };
            Assert.AreEqual(expected, c.GetCountry());
        }

        [TestCase("currency here")]
        [TestCase("404")]
        [TestCase("No name")]
        public void GetCurrencyTest(string expected)
        {
            var c = new CdCsConfiguration()
            {
                Country = new Country
                {
                    Currency = new Currency
                    {
                        Name = expected
                    }
                }
            };
            Assert.AreEqual(expected, c.GetCurrency());
        }

        [TestCase(
            @"Russia",
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CNEE",
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CNEE/CD_CS_Pricelist/Forms/AllItems.aspx"
        )]
        [TestCase(
            @"Austria",
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CGER",
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CGER/CD_CS_Pricelist/Forms/AllItems.aspx"
        )]
        [TestCase(
            @"404",
            @"some_path_xxx/some_region__yyy",
            @"some_path_xxx/some_region__yyy/CD_CS_Pricelist/Forms/AllItems.aspx"
        )]
        [TestCase(
            @"United States",
            @"https://partners.ts.fujitsu.com/teams/cor/SCD/USA/test",
            @"https://partners.ts.fujitsu.com/teams/cor/SCD/USA/test/CD_CS_Pricelist/Forms/AllItems.aspx"
        )]
        public void GetPriceListPath(string country, string fileWebUrl, string expected)
        {
            var c = new CdCsConfiguration()
            {
                Country = new Country { Name = country },
                FileWebUrl = fileWebUrl
            }; 
            Assert.AreEqual(expected, c.GetPriceListPath());
        }
    }
}
