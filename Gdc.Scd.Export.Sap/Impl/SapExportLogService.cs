using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Sap.Enitities;
using Gdc.Scd.Export.Sap.Interfaces;
using System;
using System.Linq;
using Gdc.Scd.Export.Sap.Dto;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class SapExportLogService : ReadingDomainService<SapExportLog>, ISapExportLogService
    {
        public SapExportLogService(IRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public void Log(ExportType exportType, DateTime uploadDate, DateTime? startPeriod, int fieNumber, bool isSend)
        {
            var log = new SapExportLog
            {
                UploadDate = uploadDate,
                PeriodStartDate = startPeriod,
                ExportType = (int) exportType,
                FileNumber = fieNumber,
                IsSend = isSend
            };

            this.repository.Save(log);
            this.repositorySet.Sync();
        }

        public void DeleteOverYear()
        {
            var date = DateTime.UtcNow.AddYears(-1);
            var ids = this.GetAll().Where(item => item.UploadDate < date).Select(item => item.Id);

            foreach (var id in ids)
            {
                this.repository.Delete(id);
            }

            this.repositorySet.Sync();
        }
    }
}
