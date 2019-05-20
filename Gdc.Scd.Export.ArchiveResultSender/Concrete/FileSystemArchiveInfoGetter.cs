using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.ArchiveResultSender.Abstract;
using NLog;

namespace Gdc.Scd.Export.ArchiveResultSender.Concrete
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
