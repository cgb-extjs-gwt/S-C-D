using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Sap.Enitities;
using Gdc.Scd.Export.Sap.Interfaces;
using System;
using System.Linq;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class SapExportLogService : ReadingDomainService<SapExportLog>, ISapExportLogService
    {
        public SapExportLogService(IRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public void Log(ExportType exportType)
        {
            var log = new SapExportLog
            {
                ExportType = exportType,
                DateTime = DateTime.UtcNow
            };

            this.repository.Save(log);
            this.repositorySet.Sync();
        }

        public void DeleteOverYear()
        {
            var date = DateTime.UtcNow.AddYears(-1);
            var ids = this.GetAll().Where(item => item.DateTime < date).Select(item => item.Id);

            foreach (var id in ids)
            {
                this.repository.Delete(id);
            }

            this.repositorySet.Sync();
        }
    }
}
