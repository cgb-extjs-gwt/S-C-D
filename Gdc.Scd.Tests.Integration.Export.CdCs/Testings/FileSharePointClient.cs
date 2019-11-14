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
        
        private const string EXCEL = "CD_CS_Master File_SCD 2.0_v3.xlsm";

        public FileSharePointClient() : base(null) { }

        public override MemoryStream Load(SpFileDto cfg)
        {
            return ExcelWriterTest.GetDoc().Copy();
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
