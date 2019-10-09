using ClosedXML.Excel;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gdc.Scd.Export.CdCs.Helpers
{
    public class ExcelWriter : IDisposable
    {
        private class InputMctCdCsWGsColumns
        {
            public const int ServiceLocation = 1;
            public const int Availability = 2;
            public const int ReactionTime = 3;
            public const int ReactionType = 4;
            public const int WarrantyGroup = 5;
            public const int Duration = 6;
            public const int ServiceTC = 7;
            public const int ServiceTP = 8;
            public const int ServiceTP_MonthlyYear1 = 9;
            public const int ServiceTP_MonthlyYear2 = 10;
            public const int ServiceTP_MonthlyYear3 = 11;
            public const int ServiceTP_MonthlyYear4 = 12;
            public const int ServiceTP_MonthlyYear5 = 13;
        }

        private class ProActiveOutputColumns
        {
            public const int Wg = 1;
            public const int ProActive6 = 2;
            public const int ProActive7 = 3;
            public const int ProActive3 = 4;
            public const int ProActive4 = 5;
            public const int OneTimeTask = 6;
        }

        private class HddRetentionColumns
        {
            public const int Wg = 1;
            public const int WgName = 2;
            public const int TP = 3;
            public const int DealerPrice = 4;
            public const int ListPrice = 5;
        }

        private readonly IXLWorkbook workbook;

        public ExcelWriter(Stream data)
        {
            this.workbook = new XLWorkbook(data);
        }

        public void Dispose()
        {
            if (workbook != null)
            {
                workbook.Dispose();
            }
        }

        public Stream GetData()
        {
            var stream = new MemoryStream(1024 * 1024 * 2); //2MB
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            Dispose();
            return stream;
        }

        public SlaCollection ReadSla()
        {
            var data = new SlaCollection(128);

            var sheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs);

            var range = sheet.RangeUsed();
            var rowCount = range.RowCount();

            for (var row = 2; row <= rowCount; row++)
            {
                data.Add(new SlaDto
                {
                    ServiceLocation = sheet.Cell(row, InputMctCdCsWGsColumns.ServiceLocation).Value.ToString(),
                    Availability = sheet.Cell(row, InputMctCdCsWGsColumns.Availability).Value.ToString(),
                    ReactionTime = sheet.Cell(row, InputMctCdCsWGsColumns.ReactionTime).Value.ToString(),
                    ReactionType = sheet.Cell(row, InputMctCdCsWGsColumns.ReactionType).Value.ToString(),
                    WarrantyGroup = sheet.Cell(row, InputMctCdCsWGsColumns.WarrantyGroup).Value.ToString(),
                    Duration = sheet.Cell(row, InputMctCdCsWGsColumns.Duration).Value.ToString(),
                });
            }

            return data;
        }

        public void WriteTcTp(List<ServiceCostDto> data)
        {
            var sheet = workbook.Worksheet(InputSheets.InputMctCdCsWGs);

            sheet.Column(1).InsertColumnsBefore(2);

            sheet.SetCellAsString(1, 1, "Key");
            sheet.SetCellAsString(1, 2, "Country Group");
            sheet.Row(1).Style.Font.Bold = true;

            const int startRow = 2;

            sheet.ClearFrom(startRow);

            for (int i = 0, row = startRow; i < data.Count; i++, row++)
            {
                var cost = data[i];

                sheet.SetCellAsString(row, 1, cost.Key);
                sheet.SetCellAsString(row, 2, cost.CountryGroup);
                sheet.SetCellAsString(row, 2 + InputMctCdCsWGsColumns.ServiceLocation, cost.ServiceLocation);
                sheet.SetCellAsString(row, 2 + InputMctCdCsWGsColumns.Availability, cost.Availability);
                sheet.SetCellAsString(row, 2 + InputMctCdCsWGsColumns.ReactionTime, cost.ReactionTime);
                sheet.SetCellAsString(row, 2 + InputMctCdCsWGsColumns.ReactionType, cost.ReactionType);
                sheet.SetCellAsString(row, 2 + InputMctCdCsWGsColumns.WarrantyGroup, cost.WarrantyGroup);
                sheet.SetCellAsString(row, 2 + InputMctCdCsWGsColumns.Duration, cost.Duration);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTC, cost.ServiceTC);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTP, cost.ServiceTP);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1, cost.ServiceTP_MonthlyYear1);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2, cost.ServiceTP_MonthlyYear2);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3, cost.ServiceTP_MonthlyYear3);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4, cost.ServiceTP_MonthlyYear4);
                sheet.SetCellAsDouble(row, 2 + InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5, cost.ServiceTP_MonthlyYear5);
            }
        }

        public void WriteProactive(List<ProActiveDto> data, string currency)
        {
            var sheet = workbook.Worksheet(InputSheets.ProActiveOutput);

            for (int i = 0, rowNum = 8; i < data.Count; i++, rowNum++)
            {
                var pro = data[i];

                sheet.SetCellAsString(rowNum, ProActiveOutputColumns.Wg, pro.Wg);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive6, pro.ProActive6, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive7, pro.ProActive7, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive3, pro.ProActive3, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive4, pro.ProActive4, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.OneTimeTask, pro.OneTimeTasks, currency);
            }
        }

        public void WriteHdd(List<HddRetentionDto> data, string currency)
        {
            var sheet = workbook.Worksheet(InputSheets.HddRetention);

            const int startRow = 4;

            sheet.ClearFrom(startRow);

            //set Last update
            var today = DateTime.Today.ToString("dd.MM.yyyy");
            sheet.SetCellAsString(1, HddRetentionColumns.ListPrice, $"Last update: {today}");

            for (int i = 0, row = startRow; i < data.Count; i++, row++)
            {
                var hdd = data[i];

                sheet.SetCellAsString(row, HddRetentionColumns.Wg, hdd.Wg);
                sheet.SetCellAsString(row, HddRetentionColumns.WgName, hdd.WgName ?? string.Empty);
                sheet.SetCellAsCurrency(row, HddRetentionColumns.TP, hdd.TransferPrice, currency);
                sheet.SetCellAsCurrency(row, HddRetentionColumns.DealerPrice, hdd.DealerPrice, currency);
                sheet.SetCellAsCurrency(row, HddRetentionColumns.ListPrice, hdd.ListPrice, currency);
            }
        }
    }
}
