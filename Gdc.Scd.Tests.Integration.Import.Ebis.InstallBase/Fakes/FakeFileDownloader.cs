using System;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase.Fakes
{
    class FakeFileDownloader : FileDownloader
    {
        public override void MoveFile(DownloadInfoDto info) { }

        public override DateTime? GetModifiedDateTime(DownloadInfoDto info)
        {
            return DateTime.Now;
        }
    }
}
