using Gdc.Scd.Core.Dto;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Export.ArchiveResultSender.Abstract
{
    public interface IArchiveInfoGetter
    {
        IList<ArchiveFolderDto> GetArchiveResults(DateTime periodStart, DateTime periodEnd);
    }
}
