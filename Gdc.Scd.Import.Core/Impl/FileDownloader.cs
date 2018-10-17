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
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, 
                Path.Combine(info.ProcessedFilePath, $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.ToShortDateString()}{Path.GetExtension(fileName)}"), true);
            File.Delete(filePath);
        }

        private string CheckFile(DownloadInfoDto info)
        {
            var files = Directory.GetFiles(info.Path, info.File);
            if (files.Length == 0)
                throw new FileNotFoundException("File cannot be found", info.File);
            var file =  files.Select(f => new FileInfo(Path.Combine(info.Path, f))).OrderByDescending(f => f.LastWriteTime).First();
            return file.FullName;
        }
    }
}
