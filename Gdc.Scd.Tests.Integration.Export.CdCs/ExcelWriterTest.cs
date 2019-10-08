using Gdc.Scd.Export.CdCs.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class ExcelWriterTest
    {
        private ExcelWriter writer;

        [SetUp]
        public void Setup()
        {
            writer = new ExcelWriter();
        }

        [TestCase]
        public void WriteTcTpTest()
        {

        }
    }
}
