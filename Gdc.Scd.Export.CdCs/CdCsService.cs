using ClosedXML.Excel;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Enums;
using Gdc.Scd.Export.CdCs.Helpers;
using Gdc.Scd.Export.CdCs.Procedures;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Export.CdCs
{
    public class CdCsService
    {
        private IKernel Kernel;

        private SharePointClient spClient;

        private ILogger Logger;

        public CdCsService(
                IKernel kernel,
                SharePointClient spClient,
                ILogger log
            )
        {
            this.Kernel = kernel;
            this.spClient = spClient;
            this.Logger = log;
        }

        public virtual void Run()
        {
            Logger.Info(CdCsMessages.START_PROCESS);
            Logger.Info(CdCsMessages.READ_TEMPLATE);

            var excel = spClient.Load(Config.SpFile);

            Logger.Info(CdCsMessages.PARSE_SLA_SHEET);
            var slaList = ReadSla(excel);

            Logger.Info(CdCsMessages.WRITE_COUNTRY_COSTS);
            FillCdCs(excel, slaList);

            Logger.Info(CdCsMessages.END_PROCESS);
        }

        protected virtual List<SlaDto> ReadSla(Stream excel)
        {
            var slaList = new List<SlaDto>();

            using (var workbook = new XLWorkbook(excel))
            using (var inputSheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs))
            {
                var range = inputSheet.RangeUsed();
                var rowCount = range.RowCount();

                for (var row = 2; row <= rowCount; row++)
                {
                    slaList.Add(new SlaDto
                    {
                        ServiceLocation = inputSheet.Cell(row, InputMctCdCsWGsColumns.ServiceLocation).Value.ToString(),
                        Availability = inputSheet.Cell(row, InputMctCdCsWGsColumns.Availability).Value.ToString(),
                        ReactionTime = inputSheet.Cell(row, InputMctCdCsWGsColumns.ReactionTime).Value.ToString(),
                        ReactionType = inputSheet.Cell(row, InputMctCdCsWGsColumns.ReactionType).Value.ToString(),
                        WarrantyGroup = inputSheet.Cell(row, InputMctCdCsWGsColumns.WarrantyGroup).Value.ToString(),
                        Duration = inputSheet.Cell(row, InputMctCdCsWGsColumns.Duration).Value.ToString(),
                    });
                }
                excel.Seek(0, SeekOrigin.Begin);
            }

            return slaList;
        }

        protected void FillCdCs(Stream excel, List<SlaDto> slaList)
        {
            Logger.Info(CdCsMessages.READ_CONFIGURATION);
            var configHandler = Kernel.Get<ConfigHandler>();
            var configList = configHandler.ReadAllConfiguration().Take(3);

            var getServiceCostsBySla = Kernel.Get<GetServiceCostsBySla>();
            var getProActiveCosts = Kernel.Get<GetProActiveCosts>();
            var getHddRetentionCosts = Kernel.Get<GetHddRetentionCosts>();

            foreach (var config in configList)
            {
                var country = config.Country.Name;
                var currency = config.Country.Currency.Name;
                Logger.Info(CdCsMessages.READ_COUNTRY_COSTS, country);

                Logger.Info(CdCsMessages.READ_SERVICE);
                var costsList = getServiceCostsBySla.Execute(country, slaList);

                Logger.Info(CdCsMessages.READ_PROACTIVE);
                var proActiveList = getProActiveCosts.Execute(country);

                Logger.Info(CdCsMessages.READ_HDD_RETENTION);
                var hddRetention = getHddRetentionCosts.Execute(country);

                using (var workbook = new XLWorkbook(excel))
                using (var inputMctSheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs))
                using (var proActiveSheet = workbook.Worksheet(InputSheets.ProActiveOutput))
                using (var hddRetentionSheet = workbook.Worksheet(InputSheets.HddRetention))
                {
                    //SERVICE COSTS
                    var range = inputMctSheet.RangeUsed();
                    for (var row = 2; row <= range.RowCount(); row++)
                    {
                        range.Row(row).Clear();
                    }
                    Logger.Info(CdCsMessages.WRITE_SERVICE);
                    var rowNum = 2;
                    foreach (var cost in costsList)
                    {
                        inputMctSheet.SetCellAsString(rowNum, InputMctCdCsWGsColumns.ServiceLocation, cost.Sla.ServiceLocation);
                        inputMctSheet.SetCellAsString(rowNum, InputMctCdCsWGsColumns.Availability, cost.Sla.Availability);
                        inputMctSheet.SetCellAsString(rowNum, InputMctCdCsWGsColumns.ReactionTime, cost.Sla.ReactionTime);
                        inputMctSheet.SetCellAsString(rowNum, InputMctCdCsWGsColumns.ReactionType, cost.Sla.ReactionType);
                        inputMctSheet.SetCellAsString(rowNum, InputMctCdCsWGsColumns.WarrantyGroup, cost.Sla.WarrantyGroup);
                        inputMctSheet.SetCellAsString(rowNum, InputMctCdCsWGsColumns.Duration, cost.Sla.Duration);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTC, cost.ServiceTC);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTP, cost.ServiceTP);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1, cost.ServiceTP_MonthlyYear1);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2, cost.ServiceTP_MonthlyYear2);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3, cost.ServiceTP_MonthlyYear3);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4, cost.ServiceTP_MonthlyYear4);
                        inputMctSheet.SetCellAsDouble(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5, cost.ServiceTP_MonthlyYear5);

                        rowNum++;
                    }

                    //PROACTIVE COSTS
                    Logger.Info(CdCsMessages.WRITE_PROACTIVE);
                    rowNum = 8;
                    foreach (var pro in proActiveList)
                    {
                        proActiveSheet.Row(rowNum).Clear();
                        proActiveSheet.SetCellAsString(rowNum, ProActiveOutputColumns.Wg, pro.Wg);
                        proActiveSheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive6, pro.ProActive6, currency);
                        proActiveSheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive7, pro.ProActive7, currency);
                        proActiveSheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive3, pro.ProActive3, currency);
                        proActiveSheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive4, pro.ProActive4, currency);
                        proActiveSheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.OneTimeTask, pro.OneTimeTasks, currency);

                        rowNum++;
                    }

                    //HDD RETENTION
                    Logger.Info(CdCsMessages.WRITE_HDD_RETENTION);
                    range = hddRetentionSheet.RangeUsed();
                    for (var row = 4; row < range.RowCount(); row++)
                    {
                        range.Row(row).Clear();
                    }

                    //set Last update
                    var today = DateTime.Today.ToString("dd.MM.yyyy");
                    hddRetentionSheet.SetCellAsString(1, HddRetentionColumns.ListPrice, $"Last update: {today}");

                    rowNum = 4;

                    foreach (var hdd in hddRetention)
                    {
                        hddRetentionSheet.SetCellAsString(rowNum, HddRetentionColumns.Wg, hdd.Wg);
                        hddRetentionSheet.SetCellAsString(rowNum, HddRetentionColumns.WgName, hdd.WgName ?? string.Empty);
                        hddRetentionSheet.SetCellAsCurrency(rowNum, HddRetentionColumns.TP, hdd.TransferPrice, currency);
                        hddRetentionSheet.SetCellAsCurrency(rowNum, HddRetentionColumns.DealerPrice, hdd.DealerPrice, currency);
                        hddRetentionSheet.SetCellAsCurrency(rowNum, HddRetentionColumns.ListPrice, hdd.ListPrice, currency);
                        rowNum++;
                    }

                    //UPLOAD
                    Logger.Info(CdCsMessages.UPLOAD_FILE);
                    workbook.SaveAs(excel);
                    //
                    spClient.Send(excel, config);
                }
            }

            excel.Dispose();
        }
    }
}
