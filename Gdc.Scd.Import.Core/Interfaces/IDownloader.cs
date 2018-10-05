using Gdc.Scd.Import.Core.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IDownloader
    {
        StreamReader DownloadData(DownloadInfoDto info);
        DateTime GetModifiedDateTime(DownloadInfoDto info);
        void MoveFile(DownloadInfoDto info);
    }
}
