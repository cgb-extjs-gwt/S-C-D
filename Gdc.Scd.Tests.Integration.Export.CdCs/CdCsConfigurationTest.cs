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
    }
}
