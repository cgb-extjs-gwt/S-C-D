using Gdc.Scd.BusinessLogicLayer.Helpers;
using NUnit.Framework;

namespace Gdc.Scd.Tests.BusinessLogicLayer.Helpers
{
    public class ReportFormatterUnitTest
    {
        ///Check double string formatter with 4 decimals
        [TestCase(000, "0")]
        [TestCase(1, "1")]
        [TestCase(123.45689, "123.4569")]
        [TestCase(10.3, "10.3")]
        public void Format4Decimals_Test(double v, string expected)
        {
            Assert.AreEqual(expected, ReportFormatter.Format4Decimals(v));
        }

        ///Check double EUR string formatter
        [TestCase(000, "0")]
        [TestCase(1, "1")]
        [TestCase(123.45689, "123.4569")]
        [TestCase(10.3, "10.3")]
        public void Format4Decimals_Test(double v, string expected)
        {
            Assert.AreEqual(expected, ReportFormatter.FormatEuro(v));
        }

    }
}
