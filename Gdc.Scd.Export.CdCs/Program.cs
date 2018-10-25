using ClosedXML.Excel;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Microsoft.SharePoint.Client;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Gdc.Scd.Export.CdCs.Enums;
using File = Microsoft.SharePoint.Client.File;

namespace Gdc.Scd.Export.CdCs
{
    class Program
    {
        static void Main(string[] args)
        {
            DoThingsAsync().Wait();
        }

        public static async Task DoThingsAsync()
        {
            var networkCredential = new NetworkCredential(Config.SpServiceAccount, Config.SpServicePassword, Config.SpServiceDomain);
            var downloader = new SpFileDownloader(networkCredential);
            var inputFile = new SpFileDto
            {
                WebUrl = Config.CalculatiolToolWeb,
                ListName = Config.CalculatiolToolList,
                FolderServerRelativeUrl = Config.CalculatiolToolFolder,
                FileName = Config.CalculatiolToolInputFileName
            };
            var downloadedInputFile = downloader.DownloadData(inputFile);

            var slaList = new List<SlaDto>();

            using (var memoryStream = new MemoryStream())
            {
                CopyStream(downloadedInputFile, memoryStream);
                using (var workbook = new XLWorkbook(memoryStream))
                using (var inputSheet = workbook.Worksheet(1))
                {
                    var range = inputSheet.RangeUsed();
                    var colCount = range.ColumnCount();
                    var rowCount = range.RowCount();



                    for (int row = 2; row < rowCount; row++)
                    {
                        slaList.Add(new SlaDto
                        {
                            FspCode = inputSheet.Cell(row, InputFileCoumns.FspCode).Value.ToString(),
                            Country = "China",
                            ServiceLocation = inputSheet.Cell(row, InputFileCoumns.ServiceLocation).Value.ToString(),
                            Availability = inputSheet.Cell(row, InputFileCoumns.Availability).Value.ToString(),
                            ReactionTime = inputSheet.Cell(row, InputFileCoumns.ReactionTime).Value.ToString(),
                            ReactionType = inputSheet.Cell(row, InputFileCoumns.ReactionType).Value.ToString(),
                            WarrantyGroup = inputSheet.Cell(row, InputFileCoumns.WarrantyGroup).Value.ToString(),
                            Duration = inputSheet.Cell(row, InputFileCoumns.Duration).Value.ToString(),
                        });
                        inputSheet.Cell(row, CdCsFileCoumns.ServiceTC).Value = row;
                        inputSheet.Cell(row, CdCsFileCoumns.ServiceTP).Value = rowCount - row;
                    }
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    //using (var ctx = new ClientContext(inputFile.WebUrl))
                    //{
                    //    ctx.Credentials = networkCredential;

                    //    File.SaveBinaryDirect(ctx, "/02/sites/p/Migration-GDC/Shared Documents/CD_CS calculation tool interface/China " + Config.CalculatiolToolInputFileName, memoryStream, true);
                    //}

                }
            }

            var cdCsFile = new SpFileDto
            {
                WebUrl = Config.CalculatiolToolWeb,
                ListName = Config.CalculatiolToolList,
                FolderServerRelativeUrl = Config.CalculatiolToolFolder,
                FileName = Config.CalculatiolToolFileName
            };
            var downloadedcdCsFile = downloader.DownloadData(cdCsFile);

            var countries = new List<string>() { "China", "Russia", "Germany", "Japan" };

            foreach (var country in countries)
            {
                var costsList = new List<ServiceCostDto>();
                IKernel kernel = new StandardKernel(new Module());
                var calcService = kernel.Get<CalculatorService>();
                foreach (var sla in slaList)
                {
                    var costs = await calcService.GetServiceCostsAsync(sla);
                    costsList.Add(costs);
                    Console.WriteLine("{0}\t{1}\t{2}", costs.FspCode, costs.ServiceTC, costs.ServiceTP);
                }
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(downloadedcdCsFile, memoryStream);
                    using (var workbook = new XLWorkbook(memoryStream))
                    using (var inputSheet = workbook.Worksheet(1))
                    {
                        var range = inputSheet.RangeUsed();
                        var colCount = range.ColumnCount();
                        var rowCount = range.RowCount();

                        for (int row = 2; row < rowCount; row++)
                        {
                            slaList.Add(new SlaDto
                            {
                                FspCode = inputSheet.Cell(row, InputFileCoumns.FspCode).Value.ToString(),
                                Country = country,
                                ServiceLocation = inputSheet.Cell(row, InputFileCoumns.ServiceLocation).Value.ToString(),
                                Availability = inputSheet.Cell(row, InputFileCoumns.Availability).Value.ToString(),
                                ReactionTime = inputSheet.Cell(row, InputFileCoumns.ReactionTime).Value.ToString(),
                                ReactionType = inputSheet.Cell(row, InputFileCoumns.ReactionType).Value.ToString(),
                                WarrantyGroup = inputSheet.Cell(row, InputFileCoumns.WarrantyGroup).Value.ToString(),
                                Duration = inputSheet.Cell(row, InputFileCoumns.Duration).Value.ToString(),
                            });
                            inputSheet.Cell(row, CdCsFileCoumns.ServiceTC).Value = row;
                            inputSheet.Cell(row, CdCsFileCoumns.ServiceTP).Value = rowCount - row;
                        }
                        workbook.SaveAs(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        //using (var ctx = new ClientContext(inputFile.WebUrl))
                        //{
                        //    ctx.Credentials = networkCredential;

                        //    File.SaveBinaryDirect(ctx, "/02/sites/p/Migration-GDC/Shared Documents/CD_CS calculation tool interface/China " + Config.CalculatiolToolInputFileName, memoryStream, true);
                        //}

                    }
                }
            }
        }

        public static void CopyStream(Stream source, Stream destination)
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
