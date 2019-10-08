using Gdc.Scd.Export.CdCs.Helpers;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class ExcelWriterTest
    {
        private ExcelWriter writer;

        [SetUp]
        public void Setup()
        {
            writer = new ExcelWriter(null);
        }

        [TestCase]
        public void WriteTcTpTest()
        {

        }
    }
}
