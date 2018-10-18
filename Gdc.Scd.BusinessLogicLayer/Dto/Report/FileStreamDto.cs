using System.IO;

namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class FileStreamDto
    {
        public Stream Data { get; set; }

        public string FileName { get; set; }
    }
}
