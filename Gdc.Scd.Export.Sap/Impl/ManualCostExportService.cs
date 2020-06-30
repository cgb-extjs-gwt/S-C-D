using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Export.Sap.Enitities;
using Gdc.Scd.Export.Sap.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Sap.Dto;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class ManualCostExportService : IManualCostExportService
    {
        protected ILogger Logger;
        private readonly IDomainService<SapTable> sapTableService;
        private readonly IFileService fileService;
        private readonly IRepository<HardwareManualCost> hwManualRepo;
        private readonly IRepository<User> userRepo;
        private readonly IRepositorySet repository;
        private readonly ISapExportLogService sapLogService;

        private ExportType ExportType { get; set; }
        private DateTime UploadDate => DateTime.Now;
        private DateTime? StartPeriod => this.ExportType == (ExportType.Partial) ? (DateTime?) DateTime.Today.AddDays(-1) : null;

        public ManualCostExportService(
            IRepository<HardwareManualCost> hwManualRepo,
            IRepository<User> userRepo,
            IDomainService<SapTable> sapTableService,
            ISapExportLogService sapLogService,
            IRepositorySet repo,
            ILogger logger)
        {
            this.hwManualRepo = hwManualRepo;
            this.userRepo = userRepo;
            this.sapTableService = sapTableService;
            this.sapLogService = sapLogService;
            this.repository = repo;
            this.Logger = logger;
            this.fileService = new FileService(this.Logger);
            this.ExportType = ExportType.Partial;
            
        }

        public void Export()
        {
            Logger.Info(SapLogConstants.START_PROCESS);

            var lastSapLog = 
                this.sapLogService.GetAll()
                                  .OrderBy(log => log.UploadDate)
                                  .LastOrDefault();

            Logger.Info(SapLogConstants.SAPLOG_RECEIVED);

            if (lastSapLog == null)
            {
                this.ExportType = ExportType.Full;
            }
            else if (!lastSapLog.IsSend)
            {
                Logger.Error( SapLogConstants.SAPLOG_NOTSENT + lastSapLog.FileNumber);
                return;
            }
            else if (Enum.TryParse(Config.ExportType, out ExportType exportTypeParam))
            {
                this.ExportType = exportTypeParam;
            }
            else if ((DateTime.Now - lastSapLog.UploadDate).Days != 1)
            {
                this.ExportType = ExportType.Full;
            }

            this.Do();

            Logger.Info(SapLogConstants.END_PROCESS);
        }

        private void Do()
        {
            var locapMergedData = new LocapReportService(repository).Execute(this.StartPeriod);
            if (locapMergedData == null)
            {
                Logger.Info(SapLogConstants.NODATA_FORUPLOAD);
                return;
            }

            //upload Hardware packs data
            var hwPacksResult = this.ExportPacks(locapMergedData, SapUploadPackType.HW);

            //upload StandardWarranty data
            var stdwResult = this.ExportPacks(locapMergedData, SapUploadPackType.STDW);

            if (hwPacksResult && stdwResult)
            {
                this.SetUploadedHwManualCosts(locapMergedData);
            }
        }

        private bool ExportPacks(List<LocapMergedData> locapMergedData, SapUploadPackType packType)
        {
            var lastSapLog =
                this.sapLogService.GetAll()
                    .OrderBy(log => log.UploadDate)
                    .LastOrDefault();
            if (lastSapLog != null && lastSapLog.IsSend == false)
            {
                Logger.Error(SapLogConstants.SAPLOG_PREVNOTSENT + lastSapLog.FileNumber);
                return false;
            }

            var fileNumber = lastSapLog?.FileNumber + 1 ?? 1;

            var sapUploadData = this.GetSapMappedData(locapMergedData, packType);
            var fileName = fileService.CreateFileOnServer(sapUploadData, fileNumber);
            var isSend = fileService.SendFileToSap(fileName);

            this.sapLogService.Log(this.ExportType, this.UploadDate, this.StartPeriod, fileNumber, isSend);

            return isSend;
        }

        private List<ReleasedData> GetSapMappedData(List<LocapMergedData> locapMergedDatas, SapUploadPackType packType)
        {
            var saptables = this.sapTableService.GetAll().Where(sp => sp.SapUploadPackType == packType.ToString()).ToList();
            return locapMergedDatas.Select(lp => new ReleasedData
            {
                CurrencyName = lp.Currency,
                SapDivision = lp.SapDivision,
                SapSalesOrg = lp.SapSalesOrganization,
                FspCodeWg = (packType == SapUploadPackType.HW) ? lp.FspCode : lp.WgName,
                PriceDb = (packType == SapUploadPackType.HW) ? lp.ServiceTP : lp.LocalServiceStdw,
                ValidFromDt = lp.ReleaseDate ?? (lp.NextSapUploadDate ?? DateTime.Today),
                ValidToDt = DateTime.Parse(Config.MaxDateTime, new CultureInfo("de-DE")),
                SapTable = saptables.FirstOrDefault(s => s.SapSalesOrganization.Equals(lp.SapSalesOrganization, StringComparison.InvariantCultureIgnoreCase))?.Name,
                SapItemCategory = lp.SapItemCategory,
                SapUploadPackType = packType
            }).Distinct().ToList();
        }

        private void SetUploadedHwManualCosts(List<LocapMergedData> locapMergedDatas)
        {
            var recordsId = locapMergedDatas.Select(x => x.PortfolioId);
            var sapUser = userRepo
                .GetAll()
                .FirstOrDefault(u => u.Login.Equals(Config.SapDBUserLogin, StringComparison.InvariantCultureIgnoreCase));

            var entities = (
                from hw in hwManualRepo.GetAll().Where(x => recordsId.Contains(x.Id))
                select hw).ToDictionary(x => x.Id, y => y);

            ITransaction transaction = null;
            try
            {
                transaction = repository.GetTransaction();

                foreach (var rec in locapMergedDatas)
                {
                    if (!entities.ContainsKey(rec.PortfolioId))
                    {
                        continue;
                    }

                    var hwManual = entities[rec.PortfolioId];

                        hwManual.NextSapUploadDate = null;
                        hwManual.SapUploadDate = DateTime.Now;
                        hwManual.ChangeUser = sapUser;

                        hwManualRepo.Save(hwManual);
                }

                repository.Sync();
                transaction.Commit();
            }
            catch(Exception ex)
            {
                Logger.Error(ex, SapLogConstants.HWMANUALCOSTS_CANTUPDATE);
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }

        }
    }
}
