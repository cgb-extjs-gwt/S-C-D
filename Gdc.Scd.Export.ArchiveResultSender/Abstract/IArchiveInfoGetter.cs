using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.Export.ArchiveResultSenderJob.Abstract
{
    public interface IArchiveInfoGetter
    {
        IList<ArchiveFolderDto> GetArchiveResults(DateTime periodStart, DateTime periodEnd);
    }
}
