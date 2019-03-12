using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Ninject;
using System.IO;
using System.Net;
using static Gdc.Scd.Export.CdCs.Enums;
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
        public static IKernel Kernel  { get; private set; }
        public static NetworkCredential NetworkCredential { get; private set; }
        public static SpFileDownloader Downloader { get; private set; }
        public static ILogger<LogLevel> Logger { get; private set; }

        private const string EUR_CUR = "EUR";

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
                    WebUrl = Config.CalculatiolToolWeb,
                    ListName = Config.CalculatiolToolList,
                    FolderServerRelativeUrl = Config.CalculatiolToolFolder,
                    FileName = Config.CalculatiolToolInputFileName
                };
                var downloadedInputFile = Downloader.DownloadData(inputFile);
                Logger.Log(LogLevel.Info, CdCsMessages.PARSE_SLA_FILE);
                var slaList = GetSlasFromFile(downloadedInputFile);
                Logger.Log(LogLevel.Info, CdCsMessages.READ_TEMPLATE);
                var cdCsFile = new SpFileDto
                {
                    WebUrl = Config.CalculatiolToolWeb,
                    ListName = Config.CalculatiolToolList,
                    FolderServerRelativeUrl = Config.CalculatiolToolFolder,
                    FileName = Config.CalculatiolToolFileName
                };
                var downloadedcdCsFile = Downloader.DownloadData(cdCsFile);
                Logger.Log(LogLevel.Info, CdCsMessages.WRITE_COUNTRY_COSTS);
                FillCdCsAsync(downloadedcdCsFile, slaList);
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
                    var colCount = range.ColumnCount();
                    var rowCount = range.RowCount();

                    for (int row = 2; row < rowCount; row++)
                    {
                        slaList.Add(new SlaDto
                        {
                            FspCode = inputSheet.Cell(row, InputFileCoumns.FspCode).Value.ToString(),
                            ServiceLocation = inputSheet.Cell(row, InputFileCoumns.ServiceLocation).Value.ToString(),
                            Availability = inputSheet.Cell(row, InputFileCoumns.Availability).Value.ToString(),
                            ReactionTime = inputSheet.Cell(row, InputFileCoumns.ReactionTime).Value.ToString(),
                            ReactionType = inputSheet.Cell(row, InputFileCoumns.ReactionType).Value.ToString(),
                            WarrantyGroup = inputSheet.Cell(row, InputFileCoumns.WarrantyGroup).Value.ToString(),
                            Duration = inputSheet.Cell(row, InputFileCoumns.Duration).Value.ToString(),
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

            var getHddRetentionCosts = Kernel.Get<GetHddRetentionCosts>();
            Logger.Log(LogLevel.Info, CdCsMessages.READ_HDD_RETENTION);
            var hddRetention = getHddRetentionCosts.Execute();

            foreach (var config in configList)
            {
                var country = config.Country.Name;
                var currency = config.Country.Currency.Name;
                Logger.Log(LogLevel.Info, CdCsMessages.READ_COUNTRY_COSTS, country);
                var costsList = new List<ServiceCostDto>();
               
                var getServiceCostsBySla = Kernel.Get<GetServiceCostsBySla>();
                var getProActiveCosts = Kernel.Get<GetProActiveCosts>();
               
                Logger.Log(LogLevel.Info, CdCsMessages.READ_SERVICE);
                costsList = getServiceCostsBySla.Execute(country, slaList);

                Logger.Log(LogLevel.Info, CdCsMessages.READ_PROACTIVE);
                var proActiveList = getProActiveCosts.Execute(country);
               
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
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.CountryGroup).SetValue(country);
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.FspCode).SetValue(cost.FspCode);
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTC).SetValue(FormatCostValue(cost.ServiceTC, format:"0.0000"));
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP).SetValue(FormatCostValue(cost.ServiceTP, format: "0.0000")); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1).SetValue(FormatCostValue(cost.ServiceTP_MonthlyYear1, format: "0.0000")); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2).SetValue(FormatCostValue(cost.ServiceTP_MonthlyYear2, format: "0.0000")); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3).SetValue(FormatCostValue(cost.ServiceTP_MonthlyYear3, format: "0.0000")); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4).SetValue(FormatCostValue(cost.ServiceTP_MonthlyYear4, format: "0.0000")); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5).SetValue(FormatCostValue(cost.ServiceTP_MonthlyYear5, format: "0.0000")); 
                        rowNum++;
                    }
                    Logger.Log(LogLevel.Info, CdCsMessages.WRITE_PROACTIVE);
                    rowNum = 8;
                    foreach (var pro in proActiveList)
                    {
                        proActiveSheet.Row(rowNum).Clear();
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.Wg).SetValue(pro.Wg);
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive6).SetValue(FormatCostValue(pro.ProActive6, currency));
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive7).SetValue(FormatCostValue(pro.ProActive7, currency));
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive3).SetValue(FormatCostValue(pro.ProActive3, currency));
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive4).SetValue(FormatCostValue(pro.ProActive4, currency));
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.OneTimeTask).SetValue(FormatCostValue(pro.OneTimeTasks, currency));
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
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.Wg).SetValue(hdd.Wg);
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.WgName).SetValue(hdd.WgName ?? string.Empty);
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.TP).SetValue(FormatCostValue(hdd.TransferPrice, EUR_CUR));
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.DealerPrice).SetValue(FormatCostValue(hdd.DealerPrice, EUR_CUR)); 
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.ListPrice).SetValue(FormatCostValue(hdd.ListPrice, EUR_CUR)); 
                        rowNum++;
                    }

                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Logger.Log(LogLevel.Info, CdCsMessages.UPLOAD_FILE);
                    using (var ctx = new ClientContext(config.FileWebUrl))
                    {
                        ctx.Credentials = NetworkCredential;

                        File.SaveBinaryDirect(ctx, String.Format("{0}/{1} {2}", config.FileFolderUrl, country, Config.CalculatiolToolFileName), memoryStream, true);
                    }
                }
            }

            memoryStream.Dispose();
        }

        private static string FormatCostValue(double value, string currency = "", string format = "0.00")
        {
            return value.ToString(format) + " " + currency;
        }

        private static void CopyStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[32768];
            int bytesRead;
            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);
        }
    }
}
