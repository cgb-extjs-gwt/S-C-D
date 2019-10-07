using ClosedXML.Excel;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Enums;
using Gdc.Scd.Export.CdCs.Procedures;
using Microsoft.SharePoint.Client;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SharePointFile = Microsoft.SharePoint.Client.File;

namespace Gdc.Scd.Export.CdCs
{
    public class CdCsService
    {
        public IKernel Kernel { get; }

        public NetworkCredential NetworkCredential { get; }

        public SpFileDownloader Downloader { get; }

        public ILogger Logger { get; }

        private CdCsService()
        {
            Kernel = new StandardKernel(new Module());
            NetworkCredential = new NetworkCredential(Config.SpServiceAccount, Config.SpServicePassword, Config.SpServiceDomain);
            Downloader = new SpFileDownloader(NetworkCredential);
            Logger = Kernel.Get<ILogger>();
        }

        public CdCsService(
                NetworkCredential NetworkCredential,
                SpFileDownloader Downloader,
                ILogger log
            )
        {

        }

        public virtual void Run()
        {
            Logger.Info(CdCsMessages.START_PROCESS);

            Logger.Info(CdCsMessages.READ_TEMPLATE);
            var cdCsFile = new SpFileDto
            {
                WebUrl = Config.CalculationToolWeb,
                ListName = Config.CalculationToolList,
                FolderServerRelativeUrl = Config.CalculationToolFolder,
                FileName = Config.CalculationToolFileName
            };
            var downloadedCdCsFile = Downloader.DownloadData(cdCsFile);
            var masterFileStream = new MemoryStream();
            CopyStream(downloadedCdCsFile, masterFileStream);

            Logger.Info(CdCsMessages.PARSE_SLA_SHEET);
            var slaList = GetSlasFromFile(masterFileStream);

            Logger.Info(CdCsMessages.WRITE_COUNTRY_COSTS);
            FillCdCsAsync(masterFileStream, slaList);

            Logger.Info(CdCsMessages.END_PROCESS);
        }

        private static List<SlaDto> GetSlasFromFile(Stream masterFileStream)
        {
            var slaList = new List<SlaDto>();

            using (var workbook = new XLWorkbook(masterFileStream))
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
                masterFileStream.Seek(0, SeekOrigin.Begin);
            }

            return slaList;
        }

        private void FillCdCsAsync(Stream masterFileStream, List<SlaDto> slaList)
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

                using (var workbook = new XLWorkbook(masterFileStream))
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
                        SetCellAsString(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceLocation, cost.Sla.ServiceLocation);
                        SetCellAsString(inputMctSheet, rowNum, InputMctCdCsWGsColumns.Availability, cost.Sla.Availability);
                        SetCellAsString(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ReactionTime, cost.Sla.ReactionTime);
                        SetCellAsString(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ReactionType, cost.Sla.ReactionType);
                        SetCellAsString(inputMctSheet, rowNum, InputMctCdCsWGsColumns.WarrantyGroup, cost.Sla.WarrantyGroup);
                        SetCellAsString(inputMctSheet, rowNum, InputMctCdCsWGsColumns.Duration, cost.Sla.Duration);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTC, cost.ServiceTC);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTP, cost.ServiceTP);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1, cost.ServiceTP_MonthlyYear1);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2, cost.ServiceTP_MonthlyYear2);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3, cost.ServiceTP_MonthlyYear3);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4, cost.ServiceTP_MonthlyYear4);
                        SetCellAsDouble(inputMctSheet, rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5, cost.ServiceTP_MonthlyYear5);

                        rowNum++;
                    }

                    //PROACTIVE COSTS
                    Logger.Info(CdCsMessages.WRITE_PROACTIVE);
                    rowNum = 8;
                    foreach (var pro in proActiveList)
                    {
                        proActiveSheet.Row(rowNum).Clear();
                        SetCellAsString(proActiveSheet, rowNum, ProActiveOutputColumns.Wg, pro.Wg);
                        SetCellAsCurrency(proActiveSheet, rowNum, ProActiveOutputColumns.ProActive6, pro.ProActive6, currency);
                        SetCellAsCurrency(proActiveSheet, rowNum, ProActiveOutputColumns.ProActive7, pro.ProActive7, currency);
                        SetCellAsCurrency(proActiveSheet, rowNum, ProActiveOutputColumns.ProActive3, pro.ProActive3, currency);
                        SetCellAsCurrency(proActiveSheet, rowNum, ProActiveOutputColumns.ProActive4, pro.ProActive4, currency);
                        SetCellAsCurrency(proActiveSheet, rowNum, ProActiveOutputColumns.OneTimeTask, pro.OneTimeTasks, currency);

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
                    SetCellAsString(hddRetentionSheet, 1, HddRetentionColumns.ListPrice, $"Last update: {today}");

                    rowNum = 4;

                    foreach (var hdd in hddRetention)
                    {
                        SetCellAsString(hddRetentionSheet, rowNum, HddRetentionColumns.Wg, hdd.Wg);
                        SetCellAsString(hddRetentionSheet, rowNum, HddRetentionColumns.WgName, hdd.WgName ?? string.Empty);
                        SetCellAsCurrency(hddRetentionSheet, rowNum, HddRetentionColumns.TP, hdd.TransferPrice, currency);
                        SetCellAsCurrency(hddRetentionSheet, rowNum, HddRetentionColumns.DealerPrice, hdd.DealerPrice, currency);
                        SetCellAsCurrency(hddRetentionSheet, rowNum, HddRetentionColumns.ListPrice, hdd.ListPrice, currency);
                        rowNum++;
                    }

                    //UPLOAD
                    Logger.Info(CdCsMessages.UPLOAD_FILE);
                    workbook.SaveAs(masterFileStream);
                    masterFileStream.Seek(0, SeekOrigin.Begin);
                    using (var ctx = new ClientContext(config.FileWebUrl))
                    {
                        ctx.Credentials = NetworkCredential;

                        SharePointFile.SaveBinaryDirect(ctx, $"{config.FileFolderUrl}/{country} {Config.CalculationToolFileName}",
                            masterFileStream, true);
                    }
                }
            }

            void SetCellAsString(IXLWorksheet sheet, int row, int column, string value)
            {
                sheet.Cell(row, column).SetValue<string>(value);
            }

            void SetCellAsDouble(IXLWorksheet sheet, int row, int column, double value)
            {
                sheet.Cell(row, column).SetValue<double>(value);
                sheet.Cell(row, column).Style.NumberFormat.Format = "0.0000"; ;
            }

            void SetCellAsCurrency(IXLWorksheet sheet, int row, int column, double value, string cur)
            {
                sheet.Cell(row, column).SetValue<double>(value);
                sheet.Cell(row, column).Style.NumberFormat.Format = $"0.00\\ [${cur}]";
            }

            masterFileStream.Dispose();
        }

        private static void CopyStream(Stream source, Stream destination)
        {
            var buffer = new byte[32768];
            int bytesRead;
            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);
        }
    }
}
