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

namespace Gdc.Scd.Export.CdCs.Impl
{
    public class CdCsService
    {
        public NetworkCredential NetworkCredential { get; private set; }
        public SpFileDownloader Downloader { get; private set; }

        public CdCsService()
        {
            IKernel kernel = new StandardKernel(new Module());
            NetworkCredential = new NetworkCredential(Config.SpServiceAccount, Config.SpServicePassword, Config.SpServiceDomain);
            Downloader = new SpFileDownloader(NetworkCredential);
        }

        public void DoThings()
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

                var countries = new List<string>() { "China" };

                FillCdCsAsync(downloadedcdCsFile, countries, slaList);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        private List<SlaDto> GetSlasFromFile(Stream inputFileStream)
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

        private void FillCdCsAsync(Stream cdCsFileStream, List<string> countries, List<SlaDto> slaList)
        {
            var memoryStream = new MemoryStream();
            CopyStream(cdCsFileStream, memoryStream);

            foreach (var country in countries)
            {
                var costsList = new List<ServiceCostDto>();
                IKernel kernel = new StandardKernel(new Module());
                var calcService = kernel.Get<CalculatorService>();

                foreach (var sla in slaList)
                {
                    var costs = calcService.GetServiceCostsAsync(country, sla);
                    costsList.Add(costs);
                }

                var proActiveList = calcService.GetProActiveCostsAsync(country);

                using (var workbook = new XLWorkbook(memoryStream))
                using (var inputMctSheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs))
                using (var proActiveSheet = workbook.Worksheet(InputSheets.ProActiveOutput))
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
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTC).Value = cost.ServiceTC.ToString("0.00") + " " + "EUR";                      
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP).Value = cost.ServiceTP.ToString("0.00") + " " + "EUR";
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1).Value = cost.ServiceTP_MonthlyYear1.ToString("0.00") + " " + "EUR";
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2).Value = cost.ServiceTP_MonthlyYear2.ToString("0.00") + " " + "EUR";
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3).Value = cost.ServiceTP_MonthlyYear3.ToString("0.00") + " " + "EUR";
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4).Value = cost.ServiceTP_MonthlyYear4.ToString("0.00") + " " + "EUR";
                        inputMctSheet.Cell(rowNum, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5).Value = cost.ServiceTP_MonthlyYear5.ToString("0.00") + " " + "EUR";
                        rowNum++;
                    }

                    rowNum = 8;
                    foreach(var pro in proActiveList)
                    {
                        proActiveSheet.Row(rowNum).Clear();
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.Wg).Value = pro.Wg;
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive6).Value = pro.ProActive6.ToString("0.00") + " " + "EUR";
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive7).Value = pro.ProActive7.ToString("0.00") + " " + "EUR";
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive3).Value = pro.ProActive3.ToString("0.00") + " " + "EUR";
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.ProActive4).Value = pro.ProActive4.ToString("0.00") + " " + "EUR";
                        proActiveSheet.Cell(rowNum, ProActiveOutputColumns.OneTimeTask).Value = pro.OneTimeTasks.ToString("0.00") + " " + "EUR";
                        rowNum++;
                    }

                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var ctx = new ClientContext(Config.CalculatiolToolWeb))
                    {
                        ctx.Credentials = NetworkCredential;

                        File.SaveBinaryDirect(ctx, String.Format("{0}/{1} {2}", Config.CalculatiolToolFolder, country, Config.CalculatiolToolFileName), memoryStream, true);
                    }
                }
            }

            memoryStream.Dispose();
        }

        private void CopyStream(Stream source, Stream destination)
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
