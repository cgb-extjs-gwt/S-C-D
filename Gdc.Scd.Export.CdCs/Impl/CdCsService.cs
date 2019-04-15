using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Ninject;
using System.IO;
using System.Net;
using static Gdc.Scd.Export.CdCs.Enums.Enums;
using File = Microsoft.SharePoint.Client.File;
using ClosedXML.Excel;
using Gdc.Scd.Export.CdCs.Dto;
using System.Diagnostics;
using Gdc.Scd.Export.CdCs.Procedures;
using System.Linq;
using NLog;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;

namespace Gdc.Scd.Export.CdCs.Impl
{
    public static class CdCsService
    {
        public static IKernel Kernel  { get; }
        public static NetworkCredential NetworkCredential { get; }
        public static SpFileDownloader Downloader { get; }
        public static ILogger<LogLevel> Logger { get;  }

        static CdCsService()
        {
            Kernel = new StandardKernel(new Module());
            NetworkCredential = new NetworkCredential(Config.SpServiceAccount, Config.SpServicePassword, Config.SpServiceDomain);
            Downloader = new SpFileDownloader(NetworkCredential);
            Logger = Kernel.Get<ILogger<LogLevel>>();
        }

        public static OperationResult<bool> DoThings()
        {

            try
            {
                Logger.Log(LogLevel.Info, CdCsMessages.START_PROCESS);
                Logger.Log(LogLevel.Info, CdCsMessages.READ_SLA_FILE);
                var inputFile = new SpFileDto
                {
                    WebUrl = Config.CalculationToolWeb,
                    ListName = Config.CalculationToolList,
                    FolderServerRelativeUrl = Config.CalculationToolFolder,
                    FileName = Config.CalculationToolInputFileName
                };
                var downloadedInputFile = Downloader.DownloadData(inputFile);
                Logger.Log(LogLevel.Info, CdCsMessages.PARSE_SLA_FILE);
                var slaList = GetSlasFromFile(downloadedInputFile);
                Logger.Log(LogLevel.Info, CdCsMessages.READ_TEMPLATE);
                var cdCsFile = new SpFileDto
                {
                    WebUrl = Config.CalculationToolWeb,
                    ListName = Config.CalculationToolList,
                    FolderServerRelativeUrl = Config.CalculationToolFolder,
                    FileName = Config.CalculationToolFileName
                };
                var downloadedCdCsFile = Downloader.DownloadData(cdCsFile);
                Logger.Log(LogLevel.Info, CdCsMessages.WRITE_COUNTRY_COSTS);
                FillCdCsAsync(downloadedCdCsFile, slaList);
                Logger.Log(LogLevel.Info, CdCsMessages.END_PROCESS);

                var result = new OperationResult<bool>
                {
                    IsSuccess = true,
                    Result = true
                };

                return result;
            }
            catch(Exception ex)
            {
                Logger.Log(LogLevel.Fatal, ex, CdCsMessages.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(CdCsMessages.UNEXPECTED_ERROR, ex, null, null);

                var result = new OperationResult<bool>
                {
                    IsSuccess = false,
                    Result = true
                };

                return result;
            }
            
        }

        private static List<SlaDto> GetSlasFromFile(Stream inputFileStream)
        {
            var slaList = new List<SlaDto>();

            using (var inputMemoryStream = new MemoryStream())
            {
                CopyStream(inputFileStream, inputMemoryStream);
                using (var workbook = new XLWorkbook(inputMemoryStream))
                using (var inputSheet = workbook.Worksheet(InputSheets.CalculationToolInput))
                {
                    var range = inputSheet.RangeUsed();
                    var rowCount = range.RowCount();

                    for (var row = 2; row < rowCount; row++)
                    {
                        slaList.Add(new SlaDto
                        {
                            FspCode = inputSheet.Cell(row, InputFileColumns.FspCode).Value.ToString(),
                            ServiceLocation = inputSheet.Cell(row, InputFileColumns.ServiceLocation).Value.ToString(),
                            Availability = inputSheet.Cell(row, InputFileColumns.Availability).Value.ToString(),
                            ReactionTime = inputSheet.Cell(row, InputFileColumns.ReactionTime).Value.ToString(),
                            ReactionType = inputSheet.Cell(row, InputFileColumns.ReactionType).Value.ToString(),
                            WarrantyGroup = inputSheet.Cell(row, InputFileColumns.WarrantyGroup).Value.ToString(),
                            Duration = inputSheet.Cell(row, InputFileColumns.Duration).Value.ToString(),
                        });
                    }
                    inputMemoryStream.Seek(0, SeekOrigin.Begin);
                }
            }

            return slaList;
        }

        private static void FillCdCsAsync(Stream cdCsFileStream,  List<SlaDto> slaList)
        {
            var memoryStream = new MemoryStream();
            CopyStream(cdCsFileStream, memoryStream);

            Logger.Log(LogLevel.Info, CdCsMessages.READ_CONFIGURATION);
            var configHandler = Kernel.Get<ConfigHandler>();
            var configList = configHandler.ReadAllConfiguration();
          
            var getServiceCostsBySla = Kernel.Get<GetServiceCostsBySla>();
            var getProActiveCosts = Kernel.Get<GetProActiveCosts>();
            var getHddRetentionCosts = Kernel.Get<GetHddRetentionCosts>();

            foreach (var config in configList)
            {
                var country = config.Country.Name;
                var currency = config.Country.Currency.Name;
                Logger.Log(LogLevel.Info, CdCsMessages.READ_COUNTRY_COSTS, country);
                           
                Logger.Log(LogLevel.Info, CdCsMessages.READ_SERVICE);
                var costsList = getServiceCostsBySla.Execute(country, slaList);

                Logger.Log(LogLevel.Info, CdCsMessages.READ_PROACTIVE);
                var proActiveList = getProActiveCosts.Execute(country);

                Logger.Log(LogLevel.Info, CdCsMessages.READ_HDD_RETENTION);
                var hddRetention = getHddRetentionCosts.Execute(country);

                using (var workbook = new XLWorkbook(memoryStream))
                using (var inputMctSheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs))
                using (var proActiveSheet = workbook.Worksheet(InputSheets.ProActiveOutput))
                using (var hddRetentionSheet = workbook.Worksheet(InputSheets.HddRetention))
                {
                    var range = inputMctSheet.RangeUsed();
                    for (var row = 2; row <= range.RowCount(); row++)
                    {
                        range.Row(row).Clear();
                    }
                    Logger.Log(LogLevel.Info, CdCsMessages.WRITE_SERVICE);
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

                    Logger.Log(LogLevel.Info, CdCsMessages.WRITE_PROACTIVE);
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

                    Logger.Log(LogLevel.Info, CdCsMessages.WRITE_HDD_RETENTION);
                    range = hddRetentionSheet.RangeUsed();
                    for (var row = 4; row < range.RowCount(); row++)
                    {
                        range.Row(row).Clear();
                    }

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

                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Logger.Log(LogLevel.Info, CdCsMessages.UPLOAD_FILE);
                    using (var ctx = new ClientContext(config.FileWebUrl))
                    {
                        ctx.Credentials = NetworkCredential;

                        File.SaveBinaryDirect(ctx, $"{config.FileFolderUrl}/{country} {Config.CalculationToolFileName}", memoryStream, true);
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

            memoryStream.Dispose();
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
