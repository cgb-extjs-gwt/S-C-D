using System.IO;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.CdCs;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Tests.Util;

namespace Gdc.Scd.Tests.Integration.Export.CdCs.Testings
{
    public class FileSharePointClient : SharePointClient
    {
        public const string TEST_PATH = "TestData";
        
        public const string EXCEL = "CalculationTool_CD_CS.xlsm";

        public FileSharePointClient() : base(null) { }

        public override MemoryStream Load(SpFileDto cfg)
        {
            return GetDoc().Copy();
        }

        public override void Send(Stream data, CdCsConfiguration config)
        {
            ExcelWriterTest.Save(data, config.Country.Name + "_" + EXCEL);
        }

        public static System.IO.Stream GetDoc()
        {
            return StreamUtil.ReadBin(TEST_PATH, EXCEL);
        }
    }
}
