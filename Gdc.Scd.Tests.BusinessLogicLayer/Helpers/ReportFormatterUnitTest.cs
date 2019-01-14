using Gdc.Scd.BusinessLogicLayer.Helpers;
using NUnit.Framework;

namespace Gdc.Scd.Tests.BusinessLogicLayer.Helpers
{
    public class ReportFormatterUnitTest
    {
        ///Check double string formatter with 4 decimals
        [TestCase(000, "0")]
        [TestCase(1.0, "1")]
        [TestCase(123.45689, "123.4569")]
        [TestCase(10.3, "10.3")]
        [TestCase(6667.88888, "6667.8889")]
        public void Format4Decimals_Test(double v, string expected)
        {
            Assert.AreEqual(expected, ReportFormatter.Format4Decimals(v));
        }

        ///Check double EUR string formatter
        [TestCase(0.0, "0.00 EUR")]
        [TestCase(1, "1.00 EUR")]
        [TestCase(123.45689, "123.46 EUR")]
        [TestCase(10.3, "10.30 EUR")]
        [TestCase(10500, "10500.00 EUR")]
        public void FormatEuro_Test(double v, string expected)
        {
            Assert.AreEqual(expected, ReportFormatter.FormatEuro(v));
        }

        ///Check double % string formatter like 123.456%
        [TestCase(0.0, "0.000%")]
        [TestCase(1, "1.000%")]
        [TestCase(123.45689, "123.457%")]
        [TestCase(99.99, "99.990%")]
        [TestCase(99.9999, "100.000%")]
        public void FormatPercent_Test(double v, string expected)
        {
            Assert.AreEqual(expected, ReportFormatter.FormatPercent(v));
        }

        ///Check bool string formatter like YES/NO
        [TestCase(true, "YES")]
        [TestCase(false, "NO")]
        public void FormatYesNo_Test(bool v, string expected)
        {
            Assert.AreEqual(expected, ReportFormatter.FormatYesNo(v));
        }
    }
}
