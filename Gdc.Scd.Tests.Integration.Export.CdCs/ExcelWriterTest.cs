using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Helpers;
using Gdc.Scd.Tests.Integration.Export.CdCs.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class ExcelWriterTest
    {
        public const string RESULT_PATH = "Results";
        public const string TEST_PATH = "TestData";

        private ExcelWriter writer;

        [SetUp]
        public void Setup()
        {
            writer = new ExcelWriter(FileSharePointClient.GetDoc());
        }

        [TearDown]
        public void Teardown()
        {
            writer?.Dispose();
        }

        [TestCase]
        public void ReadSlaTest()
        {
            var slas = writer.ReadSla();

            var sla = slas[1];
            Assert.AreEqual("On-Site Service", sla.ServiceLocation);
            Assert.AreEqual("24x7", sla.Availability);
            Assert.AreEqual("NBD", sla.ReactionTime);
            Assert.AreEqual("response", sla.ReactionType);
            Assert.AreEqual("CS1", sla.WarrantyGroup);
            Assert.AreEqual("3 years", sla.Duration);

            sla = slas[15];
            Assert.AreEqual("On-Site Service", sla.ServiceLocation);
            Assert.AreEqual("9x5 (local business hours)", sla.Availability);
            Assert.AreEqual("NBD", sla.ReactionTime);
            Assert.AreEqual("response", sla.ReactionType);
            Assert.AreEqual("CS4", sla.WarrantyGroup);
            Assert.AreEqual("3 years", sla.Duration);
        }

        [TestCase]
        public void WriteTcTpTest()
        {
            writer.WriteTcTp(GenTcTp());
            Save(writer.GetData(), "service_cost_");
        }

        [TestCase]
        public void WriteHddTest()
        {
            writer.WriteHdd(GenHdd(), "RUB");
            Save(writer.GetData(), "hdd_retention_");
        }

        [TestCase]
        public void WriteProTest()
        {
            writer.WriteProactive(GenPro(), "RUB");
            Save(writer.GetData(), "proactive_");
        }

        [TestCase]
        public void WriteToolConfigTest()
        {
            writer.WriteToolConfig("Russia", @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CNEE/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\Russia PriceList_CD_CS.xlsx");
            Save(writer.GetData(), "Russia PriceList_");
        }


        [TestCase]
        public void WriteDocTest()
        {
            writer.WriteTcTp(GenTcTp());
            writer.WriteProactive(GenPro(), "RUB");
            writer.WriteHdd(GenHdd(), "RUB");
            writer.WriteToolConfig("Russia", @"http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/test/CNEE/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\Russia PriceList_CD_CS.xlsx");
            Save(writer.GetData(), "cd_cs_doc_");
        }

        public static void Save(Stream stream, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn + FileSharePointClient.EXCEL, stream);
        }

        public static List<ServiceCostDto> GenTcTp()
        {
            var set = new List<ServiceCostDto>();
            var csv = StreamUtil.ReadCsv(TEST_PATH, "tc_tp.csv");

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

            return set;
        }

        public static SlaCollection GenSlaCollection()
        {
            var set = new SlaCollection(128);
            var csv = StreamUtil.ReadCsv(TEST_PATH, "tc_tp.csv");

            foreach (var line in csv)
            {
                set.Add(new SlaDto
                {
                    ServiceLocation = line.GetString(2),
                    Availability = line.GetString(3),
                    ReactionTime = line.GetString(4),
                    ReactionType = line.GetString(5),
                    WarrantyGroup = line.GetString(6),
                    Duration = line.GetString(7)
                });
            }

            return set;
        }

        public static List<HddRetentionDto> GenHdd()
        {
            var set = new List<HddRetentionDto>();
            var csv = StreamUtil.ReadCsv(TEST_PATH, "hdd.csv");

            foreach (var line in csv)
            {
                set.Add(new HddRetentionDto
                {
                    Wg = line.GetString(0),
                    WgName = line.GetString(1),
                    TransferPrice = line.GetDouble(2),
                    DealerPrice = line.GetDouble(3),
                    ListPrice = line.GetDouble(4)
                });
            }

            return set;
        }

        public static List<ProActiveDto> GenPro()
        {
            var set = new List<ProActiveDto>();
            var csv = StreamUtil.ReadCsv(TEST_PATH, "proactive.csv");

            foreach (var line in csv)
            {
                set.Add(new ProActiveDto
                {
                    Wg = line.GetString(0),
                    ProActive6 = line.GetDouble(5),
                    ProActive7 = line.GetDouble(2),
                    ProActive3 = line.GetDouble(3),
                    ProActive4 = line.GetDouble(4),
                    OneTimeTasks = line.GetDouble(1)
                });
            }

            return set;
        }
    }
}
