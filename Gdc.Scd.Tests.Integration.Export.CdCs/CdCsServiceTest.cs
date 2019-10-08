using Gdc.Scd.Export.CdCs;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System.Collections.Generic;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class CdCsServiceTest : CdCsService
    {
        public CdCsServiceTest() { }

        [TestCase]
        public void ReadSlaTest()
        {
            Assert.AreEqual(80, this.ReadSla().Count);
        }

        public List<SlaDto> ReadSla()
        {
            var s = GetSla();
            return base.ReadSla(s);
        }

        public System.IO.Stream GetSla()
        {
            return StreamUtil.ReadBin("TestData", "CalculationTool_CD_CS.xlsm");
        }
    }
}
