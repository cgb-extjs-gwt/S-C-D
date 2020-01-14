using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Export.ArchiveResultSenderJob.Abstract;

namespace Gdc.Scd.Export.ArchiveResultSenderJob.Concrete
{
    public class FileSystemArchiveInfoGetter : IArchiveInfoGetter
    {        
        public IList<ArchiveFolderDto> GetArchiveResults(DateTime periodStart, DateTime periodEnd)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Config.ScdFolder);

            var folders = from folder in dirInfo.EnumerateDirectories()
                where folder.CreationTime >= periodStart && folder.CreationTime <= periodEnd
                select new ArchiveFolderDto
                {
                    Name = folder.Name,
                    FileCount = folder.GetFiles().Length,
                    TotalFolderSize = Math.Floor(folder.GetFiles().Sum(f => f.Length) / 1024f / 1024f)
                };

            return folders.ToList();
        }
    }
}
