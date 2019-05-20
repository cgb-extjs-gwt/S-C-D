using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.Export.ArchiveResultSender.Abstract
{
    public interface IArchiveInfoGetter
    {
        IList<ArchiveFolderDto> GetArchiveResults(DateTime periodStart, DateTime periodEnd);
    }
}
