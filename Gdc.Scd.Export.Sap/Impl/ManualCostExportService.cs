using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.Export.Sap.Enitities;
using Gdc.Scd.Export.Sap.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class ManualCostExportService : IManualCostExportService
    {
        private readonly IDomainService<HardwareManualCost> manualCostService;

        private readonly IDomainService<StandardWarrantyManualCost> standardWarrantyService;

        private readonly IDomainService<SapMapping> sapMappingService;

        private readonly IDomainService<HwFspCodeTranslation> fspService;

        private readonly ISapExportLogService sapLogService;

        public ManualCostExportService(
            IDomainService<HardwareManualCost> manualCostService,
            IDomainService<StandardWarrantyManualCost> standardWarrantyService,
            IDomainService<SapMapping> sapMappingService,
            IDomainService<HwFspCodeTranslation> fspService,
            ISapExportLogService sapLogService)
        {
            this.manualCostService = manualCostService;
            this.standardWarrantyService = standardWarrantyService;
            this.sapMappingService = sapMappingService;
            this.fspService = fspService;
            this.sapLogService = sapLogService;
        }

        public void Export()
        {
            var lastSapLog = 
                this.sapLogService.GetAll()
                                  .OrderBy(log => log.DateTime)
                                  .FirstOrDefault();

            if (lastSapLog == null)
            {
                this.ExportAll();
            }
            else if (DateTime.UtcNow.Month == lastSapLog.DateTime.Month)
            {
                this.ExportByDate(lastSapLog.DateTime);
            }
            else
            {
                this.ExportAll();

                this.sapLogService.DeleteOverYear();
            }
        }

        private void ExportAll()
        {
            var manualCosts = this.manualCostService.GetAll();

            this.Export(manualCosts, ExportType.Full);
        }

        private void ExportByDate(DateTime date)
        {
            //var manualCosts =
            //    this.manualCostService.GetAll()
            //        .Where(manualCost => manualCost.SapUploadDate >= date);

            //this.Export(manualCosts, ExportType.Partial);

            throw new NotImplementedException();
        }

        private void Export(IQueryable<HardwareManualCost> manualCosts, ExportType exportType)
        {
            var releasedData = this.GetReleasedData(manualCosts);
            var excelStream = this.BuildExcelStream(releasedData);

            this.ExportFile(excelStream);

            this.sapLogService.Log(exportType);
        }

        private void ExportFile(Stream fileStream)
        {
            throw new NotImplementedException();
        }

        //private Stream BuildExcelStream(IEnumerable<HardwareManualCost> manualCosts)
        //{
        //    const int StartColumn = 1;

        //    var stream = new MemoryStream();

        //    using (var workbook = new XLWorkbook())
        //    {
        //        var sheet = workbook.Worksheets.Add("ManualCosts");

        //        var row = 1;
        //        var column = StartColumn;

        //        sheet.Cell(row, column++).Value = "Country";
        //        sheet.Cell(row, column++).Value = "Availability";
        //        sheet.Cell(row, column++).Value = "Duration";
        //        sheet.Cell(row, column++).Value = "ProActiveSla";
        //        sheet.Cell(row, column++).Value = "ReactionTime";
        //        sheet.Cell(row, column++).Value = "ReactionType";
        //        sheet.Cell(row, column++).Value = "Wg";
        //        sheet.Cell(row, column++).Value = "ServiceTP1";
        //        sheet.Cell(row, column++).Value = "ServiceTP2";
        //        sheet.Cell(row, column++).Value = "ServiceTP3";
        //        sheet.Cell(row, column++).Value = "ServiceTP4";
        //        sheet.Cell(row, column++).Value = "ServiceTP5";
        //        sheet.Cell(row, column).Value = "ServiceTP";

        //        foreach (var manulaCost in manualCosts)
        //        {
        //            row++;
        //            column = StartColumn;

        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Country.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Availability.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Duration.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.ProActiveSla.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.ReactionTime.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.ReactionType.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Wg.Name;
        //            sheet.Cell(row, column++).Value = manulaCost.ServiceTP1_Released;
        //            sheet.Cell(row, column++).Value = manulaCost.ServiceTP2_Released;
        //            sheet.Cell(row, column++).Value = manulaCost.ServiceTP3_Released;
        //            sheet.Cell(row, column++).Value = manulaCost.ServiceTP4_Released;
        //            sheet.Cell(row, column++).Value = manulaCost.ServiceTP5_Released;
        //            sheet.Cell(row, column).Value = manulaCost.ServiceTP_Released;

        //            workbook.SaveAs(stream);

        //            stream.Position = 0;
        //        }
        //    }

        //    return stream;
        //}

        private Stream BuildExcelStream(IEnumerable<ReleasedDataDto> releasedDatas)
        {
            throw new NotImplementedException();
        }

        //private IQueryable<HardwareManualCost> GetManulaCosts()
        //{
        //    return
        //        this.manualCostService.GetAll()
        //                              .Include(manualCost => manualCost.LocalPortfolio.Availability)
        //                              .Include(manualCost => manualCost.LocalPortfolio.Country)
        //                              .Include(manualCost => manualCost.LocalPortfolio.Duration)
        //                              .Include(manualCost => manualCost.LocalPortfolio.ProActiveSla)
        //                              .Include(manualCost => manualCost.LocalPortfolio.ReactionTime)
        //                              .Include(manualCost => manualCost.LocalPortfolio.ReactionType)
        //                              .Include(manualCost => manualCost.LocalPortfolio.ServiceLocation)
        //                              .Include(manualCost => manualCost.LocalPortfolio.Wg);
        //}

        private IQueryable<ReleasedDataDto> GetReleasedData(IQueryable<HardwareManualCost> manualCosts)
        {
            return
                from manualCost in manualCosts
                join fsp in this.fspService.GetAll()
                    on new { manualCost.LocalPortfolio.Sla, manualCost.LocalPortfolio.SlaHash }
                    equals new { fsp.Sla, fsp.SlaHash }
                join standartWarranty in this.standardWarrantyService.GetAll()
                    on new { manualCost.LocalPortfolio.Country, fsp.Wg }
                    equals new { standartWarranty.Country, standartWarranty.Wg }
                join sapMapping in this.sapMappingService.GetAll()
                    on manualCost.LocalPortfolio.Country
                    equals sapMapping.Country
                select new ReleasedDataDto
                {
                    CurrencyName = manualCost.LocalPortfolio.Country.Currency.Name,
                    FspCode = fsp.Name,
                    SapCountryCode = sapMapping.SapCountryName,
                    SapDivision = sapMapping.SapDivision,
                    SapSalesOrganization = sapMapping.SapSalesOrganization,
                    WgName = manualCost.LocalPortfolio.Wg.Name,
                    ServiceTP = manualCost.ServiceTP.Value,
                    StandardWarranty = standartWarranty.StandardWarranty.Value
                };
        }
    }
}
