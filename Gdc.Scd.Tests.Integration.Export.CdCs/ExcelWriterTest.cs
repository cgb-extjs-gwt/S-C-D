using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Helpers;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class ExcelWriterTest
    {
        private const string RESULT_PATH = "Results";
        private const string TEST_PATH = "TestData";

        private ExcelWriter writer;

        [SetUp]
        public void Setup()
        {
            writer = new ExcelWriter(GetDoc());
        }

        [TestCase]
        public void WriteTcTpTest()
        {
            var csv = StreamUtil.ReadCsv(TEST_PATH, "tc_tp.csv");
            var set = new List<ServiceCostDto>();

            foreach (var line in csv)
            {
                set.Add(new ServiceCostDto
                {
                    Key = line.GetString(0),
                    CountryGroup = line.GetString(1),
                    ServiceLocation = line.GetString(2),
                    Availability = line.GetString(3),
                    ReactionTime = line.GetString(4),
                    ReactionType = line.GetString(5),
                    WarrantyGroup = line.GetString(6),
                    Duration = line.GetString(7),
                    ServiceTC = line.GetDouble(8),
                    ServiceTP = line.GetDouble(9),
                    ServiceTP_MonthlyYear1 = line.GetDouble(10),
                    ServiceTP_MonthlyYear2 = line.GetDouble(11),
                    ServiceTP_MonthlyYear3 = line.GetDouble(12),
                    ServiceTP_MonthlyYear4 = line.GetDouble(13),
                    ServiceTP_MonthlyYear5 = line.GetDouble(14)
                });
            }

            writer.WriteTcTp(set);
            Save(writer.GetData(), "service_cost.xlsm");
        }

        private void Save(Stream stream, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, stream);
        }

        public System.IO.Stream GetDoc()
        {
            return StreamUtil.ReadBin(TEST_PATH, "CalculationTool_CD_CS.xlsm");
        }
    }
}
