using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
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

        private readonly ISapExportLogService sapLogService;

        public ManualCostExportService(
            IDomainService<HardwareManualCost> manualCostService,
            ISapExportLogService sapLogService)
        {
            this.manualCostService = manualCostService;
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
                this.ExportByReleaseDate(lastSapLog.DateTime);
            }
            else
            {
                this.ExportAll();

                this.sapLogService.DeleteOverYear();
            }
        }

        private void ExportAll()
        {
            var manualCosts = this.GetManulaCosts();

            this.Export(manualCosts);

            this.sapLogService.Log(ExportType.Full);
        }

        private void ExportByReleaseDate(DateTime releaseDate)
        {
            var manualCosts =
                this.GetManulaCosts()
                    .Where(manualCost => manualCost.ReleaseDate >= releaseDate);

            this.Export(manualCosts);

            this.sapLogService.Log(ExportType.Partial);
        }

        private void Export(IEnumerable<HardwareManualCost> manualCosts)
        {
            var excelStream = this.BuildExcelStream(manualCosts);

            this.ExportFile(excelStream);
        }

        private void ExportFile(Stream fileStream)
        {
            throw new NotImplementedException();
        }

        private Stream BuildExcelStream(IEnumerable<HardwareManualCost> manualCosts)
        {
            const int StartColumn = 1;

            var stream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("ManualCosts");
                
                var row = 1;
                var column = StartColumn;

                sheet.Cell(row, column++).Value = "Country";
                sheet.Cell(row, column++).Value = "Availability";
                sheet.Cell(row, column++).Value = "Duration";
                sheet.Cell(row, column++).Value = "ProActiveSla";
                sheet.Cell(row, column++).Value = "ReactionTime";
                sheet.Cell(row, column++).Value = "ReactionType";
                sheet.Cell(row, column++).Value = "Wg";
                sheet.Cell(row, column++).Value = "ServiceTP1";
                sheet.Cell(row, column++).Value = "ServiceTP2";
                sheet.Cell(row, column++).Value = "ServiceTP3";
                sheet.Cell(row, column++).Value = "ServiceTP4";
                sheet.Cell(row, column++).Value = "ServiceTP5";
                sheet.Cell(row, column).Value = "ServiceTP";

                foreach (var manulaCost in manualCosts)
                {
                    row++;
                    column = StartColumn;

                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Country.Name;
                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Availability.Name;
                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Duration.Name;
                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.ProActiveSla.Name;
                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.ReactionTime.Name;
                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.ReactionType.Name;
                    sheet.Cell(row, column++).Value = manulaCost.LocalPortfolio.Wg.Name;
                    sheet.Cell(row, column++).Value = manulaCost.ServiceTP1_Released;
                    sheet.Cell(row, column++).Value = manulaCost.ServiceTP2_Released;
                    sheet.Cell(row, column++).Value = manulaCost.ServiceTP3_Released;
                    sheet.Cell(row, column++).Value = manulaCost.ServiceTP4_Released;
                    sheet.Cell(row, column++).Value = manulaCost.ServiceTP5_Released;
                    sheet.Cell(row, column).Value = manulaCost.ServiceTP_Released;

                    workbook.SaveAs(stream);

                    stream.Position = 0;
                }
            }

            return stream;
        }

        private IQueryable<HardwareManualCost> GetManulaCosts()
        {
            return
                this.manualCostService.GetAll()
                                      .Include(manualCost => manualCost.LocalPortfolio.Availability)
                                      .Include(manualCost => manualCost.LocalPortfolio.Country)
                                      .Include(manualCost => manualCost.LocalPortfolio.Duration)
                                      .Include(manualCost => manualCost.LocalPortfolio.ProActiveSla)
                                      .Include(manualCost => manualCost.LocalPortfolio.ReactionTime)
                                      .Include(manualCost => manualCost.LocalPortfolio.ReactionType)
                                      .Include(manualCost => manualCost.LocalPortfolio.ServiceLocation)
                                      .Include(manualCost => manualCost.LocalPortfolio.Wg);
        }
    }
}
