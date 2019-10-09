using System.IO;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.CdCs;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Tests.Util;

namespace Gdc.Scd.Tests.Integration.Export.CdCs.Testings
{
    public class FileSharePointClient : SharePointClient
    {
        public FileSharePointClient() : base(null) { }

        public override MemoryStream Load(SpFileDto cfg)
        {
            return ExcelWriterTest.GetDoc().Copy();
        }

        public override void Send(Stream data, CdCsConfiguration config)
        {
            ExcelWriterTest.Save(data, $"cd_cs_doc_{config.Country.Name}.xlsm");
        }
    }
}
