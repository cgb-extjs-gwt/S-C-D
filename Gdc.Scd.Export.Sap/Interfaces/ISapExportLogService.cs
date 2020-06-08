using System;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Export.Sap.Dto;
using Gdc.Scd.Export.Sap.Enitities;

namespace Gdc.Scd.Export.Sap.Interfaces
{
    public interface ISapExportLogService : IReadingDomainService<SapExportLog>
    {
        void Log(ExportType exportType, DateTime uploadDate, DateTime? startPeriod, int fieNumber, bool isSend);

        void DeleteOverYear();
    }
}
