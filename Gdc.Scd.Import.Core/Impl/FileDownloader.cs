using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Import.Core.Impl
{
    public class FileDownloader : IDownloader
    {
        public StreamReader DownloadData(DownloadInfoDto info)
        {
            var filePath = CheckFile(info);
            if (String.IsNullOrEmpty(filePath))
                throw new FileNotFoundException(ImportConstants.COULD_NOT_FIND_FILE, info.File);
            return new StreamReader(filePath);
        }

        public virtual DateTime? GetModifiedDateTime(DownloadInfoDto info)
        {
            var filePath = CheckFile(info);
            if (String.IsNullOrEmpty(filePath))
                return null;
            var file = new FileInfo(filePath);
            return file.LastWriteTime;
        }

        public virtual void MoveFile(DownloadInfoDto info)
        {
            var filePath = CheckFile(info);
            if (String.IsNullOrEmpty(filePath))
                throw new FileNotFoundException(ImportConstants.COULD_NOT_FIND_FILE, info.File);
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, 
                Path.Combine(info.ProcessedFilePath, $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.ToShortDateString()}{Path.GetExtension(fileName)}"), true);
            File.Delete(filePath);
        }

        private string CheckFile(DownloadInfoDto info)
        {
            var files = Directory.GetFiles(info.Path, info.File);
            if (files.Length == 0)
                return String.Empty;
            var file =  files.Select(f => new FileInfo(Path.Combine(info.Path, f))).OrderBy(f => f.LastWriteTime).First();
            return file.FullName;
        }
    }
}
