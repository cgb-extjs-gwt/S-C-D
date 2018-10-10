using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class FileDownloader : IDownloader
    {
        public StreamReader DownloadData(DownloadInfoDto info)
        {
            var filePath = CheckFile(info);
            return new StreamReader(filePath);
        }

        public DateTime GetModifiedDateTime(DownloadInfoDto info)
        {
            var filePath = CheckFile(info);
            var file = new FileInfo(filePath);
            return file.LastWriteTime;
        }

        public void MoveFile(DownloadInfoDto info)
        {
            var filePath = CheckFile(info);
            File.Copy(filePath, 
                Path.Combine(info.ProcessedFilePath, $"{Path.GetFileNameWithoutExtension(info.File)}_{DateTime.Now.ToShortDateString()}{Path.GetExtension(info.File)}"), true);
            File.Delete(filePath);
        }

        private string CheckFile(DownloadInfoDto info)
        {
            var filePath = Path.Combine(info.Path, info.File);
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File cannot be found", filePath);
            return filePath;
        }
    }
}
