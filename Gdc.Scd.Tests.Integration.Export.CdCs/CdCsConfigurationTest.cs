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
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CNEE/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\Russia PriceList_CD_CS.xlsx"
        )]
        [TestCase(
            @"Austria",
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CGER",
            @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CGER/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\Austria PriceList_CD_CS.xlsx"
        )]
        [TestCase(
            @"404",
            @"some_path_xxx/some_region__yyy",
            @"some_path_xxx/some_region__yyy/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\404 PriceList_CD_CS.xlsx"
        )]
        [TestCase(
            @"United States",
            @"https://partners.ts.fujitsu.com/teams/cor/SCD/USA/test",
            @"https://partners.ts.fujitsu.com/teams/cor/SCD/USA/test/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\United States PriceList_CD_CS.xlsx"
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
