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

namespace Gdc.Scd.Export.CdCs.Impl
{
    public static class CdCsService
    {
        public static IKernel Kernel  { get; private set; }
        public static NetworkCredential NetworkCredential { get; private set; }
        public static SpFileDownloader Downloader { get; private set; }

        static CdCsService()
        {
            Kernel = new StandardKernel(new Module());
            NetworkCredential = new NetworkCredential(Config.SpServiceAccount, Config.SpServicePassword, Config.SpServiceDomain);
            Downloader = new SpFileDownloader(NetworkCredential);
        }

        public static void DoThings()
        {
            try
            {
                var inputFile = new SpFileDto
                {
                    WebUrl = Config.CalculatiolToolWeb,
                    ListName = Config.CalculatiolToolList,
                    FolderServerRelativeUrl = Config.CalculatiolToolFolder,
                    FileName = Config.CalculatiolToolInputFileName
                };
                var downloadedInputFile = Downloader.DownloadData(inputFile);

                var slaList = GetSlasFromFile(downloadedInputFile);

                var cdCsFile = new SpFileDto
                {
                    WebUrl = Config.CalculatiolToolWeb,
                    ListName = Config.CalculatiolToolList,
                    FolderServerRelativeUrl = Config.CalculatiolToolFolder,
                    FileName = Config.CalculatiolToolFileName
                };
                var downloadedcdCsFile = Downloader.DownloadData(cdCsFile);

                FillCdCsAsync(downloadedcdCsFile, slaList);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
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

            var configHandler = Kernel.Get<ConfigHandler>();
            var configList = configHandler.ReadAllConfiguration();

            foreach (var config in configList)
            {
                var country = config.Country.Name;
                var currency = config.Country.Currency.Name;

                var costsList = new List<ServiceCostDto>();
               
                var getServiceCostsBySla = Kernel.Get<GetServiceCostsBySla>();
                var getProActiveCosts = Kernel.Get<GetProActiveCosts>();
                var getHddRetentionCosts = Kernel.Get<GetHddRetentionCosts>();

                foreach (var sla in slaList)
                {
                    var costs = getServiceCostsBySla.Execute(country, sla);
                    costsList.Add(costs);
                }

                var proActiveList = getProActiveCosts.Execute(country);

                var hddRetention = getHddRetentionCosts.Execute();

                using (var workbook = new XLWorkbook(memoryStream))
                using (var inputMctSheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs))
                using (var proActiveSheet = workbook.Worksheet(InputSheets.ProActiveOutput))
                using (var hddRetentionSheet = workbook.Worksheet(InputSheets.HddRetention))
                {
                    var range = inputMctSheet.RangeUsed();
                    for (var row = 2; row < range.RowCount(); row++)
                    {
                        range.Row(row).Clear();
                    }

                    var rowNum = 2;
                    foreach (var cost in costsList)
                    {
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.CountryGroup).Value = country;
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.FspCode).Value = cost.FspCode;
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTC).Value = FormatCostValue(cost.ServiceTC, format:"0.0000");
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP).Value = FormatCostValue(cost.ServiceTP, format: "0.0000"); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1).Value = FormatCostValue(cost.ServiceTP_MonthlyYear1, format: "0.0000"); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2).Value = FormatCostValue(cost.ServiceTP_MonthlyYear2, format: "0.0000"); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3).Value = FormatCostValue(cost.ServiceTP_MonthlyYear3, format: "0.0000"); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4).Value = FormatCostValue(cost.ServiceTP_MonthlyYear4, format: "0.0000"); 
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5).Value = FormatCostValue(cost.ServiceTP_MonthlyYear5, format: "0.0000"); 
                        rowNum++;
                    }

                    rowNum = 8;
                    foreach (var pro in proActiveList)
                    {
                        proActiveSheet.Row(rowNum).Clear();
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.Wg).Value = pro.Wg;
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive6).Value = FormatCostValue(pro.ProActive6, currency);
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive7).Value = FormatCostValue(pro.ProActive7, currency);
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive3).Value = FormatCostValue(pro.ProActive3, currency);
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive4).Value = FormatCostValue(pro.ProActive4, currency);
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.OneTimeTask).Value = FormatCostValue(pro.OneTimeTasks, currency);
                        rowNum++;
                    }

                    range = hddRetentionSheet.RangeUsed();
                    for (var row = 4; row < range.RowCount(); row++)
                    {
                        range.Row(row).Clear();
                    }

                    rowNum = 4;

                    foreach (var hdd in hddRetention)
                    {
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.Wg).Value = hdd.Wg;
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.WgName).Value = hdd.WgName ?? string.Empty;
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.TP).Value = FormatCostValue(hdd.TransferPrice, currency);
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.DealerPrice).Value = FormatCostValue(hdd.DealerPrice, currency); 
                        hddRetentionSheet.Cell(rowNum, HddRetentionColumns.ListPrice).Value = FormatCostValue(hdd.ListPrice, currency); 
                        rowNum++;
                    }

                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

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
