using System.IO;

namespace Gdc.Scd.Core.Entities.Report
{
    public class ReportExportData
    {
        public Stream Data { get; set; }

        public string FileName { get; set; }
    }
}
