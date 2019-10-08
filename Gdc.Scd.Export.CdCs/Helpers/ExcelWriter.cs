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
            var stream = new MemoryStream(1028 * 256 * 8); //2MB
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

            //SERVICE COSTS
            var range = sheet.RangeUsed();

            const int startRow = 2;

            int row = startRow;
            for (; row <= range.RowCount(); row++)
            {
                range.Row(row).Clear();
            }

            row = startRow;
            for (var i = 0; i < data.Count; i++)
            {
                var cost = data[i];
                row = row + i;

                sheet.SetCellAsString(row, InputMctCdCsWGsColumns.ServiceLocation, cost.Sla.ServiceLocation);
                sheet.SetCellAsString(row, InputMctCdCsWGsColumns.Availability, cost.Sla.Availability);
                sheet.SetCellAsString(row, InputMctCdCsWGsColumns.ReactionTime, cost.Sla.ReactionTime);
                sheet.SetCellAsString(row, InputMctCdCsWGsColumns.ReactionType, cost.Sla.ReactionType);
                sheet.SetCellAsString(row, InputMctCdCsWGsColumns.WarrantyGroup, cost.Sla.WarrantyGroup);
                sheet.SetCellAsString(row, InputMctCdCsWGsColumns.Duration, cost.Sla.Duration);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTC, cost.ServiceTC);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTP, cost.ServiceTP);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear1, cost.ServiceTP_MonthlyYear1);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear2, cost.ServiceTP_MonthlyYear2);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear3, cost.ServiceTP_MonthlyYear3);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear4, cost.ServiceTP_MonthlyYear4);
                sheet.SetCellAsDouble(row, InputMctCdCsWGsColumns.ServiceTP_MonthlyYear5, cost.ServiceTP_MonthlyYear5);
            }
        }

        public void WriteProactive(List<ProActiveDto> data, string currency)
        {
            var sheet = workbook.Worksheet(InputSheets.ProActiveOutput);

            var rowNum = 8;
            for (var i = 0; i < data.Count; i++)
            {
                var pro = data[i];

                sheet.Row(rowNum).Clear();
                sheet.SetCellAsString(rowNum, ProActiveOutputColumns.Wg, pro.Wg);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive6, pro.ProActive6, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive7, pro.ProActive7, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive3, pro.ProActive3, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.ProActive4, pro.ProActive4, currency);
                sheet.SetCellAsCurrency(rowNum, ProActiveOutputColumns.OneTimeTask, pro.OneTimeTasks, currency);

                rowNum++;
            }
        }

        public void WriteHdd(List<HddRetentionDto> data, string currency)
        {
            var sheet = workbook.Worksheet(InputSheets.HddRetention);

            var range = sheet.RangeUsed();
            const int startRow = 4;

            var row = startRow;
            for (; row < range.RowCount(); row++)
            {
                range.Row(row).Clear();
            }

            //set Last update
            var today = DateTime.Today.ToString("dd.MM.yyyy");
            sheet.SetCellAsString(1, HddRetentionColumns.ListPrice, $"Last update: {today}");

            row = startRow;
            for (var i = 0; i < data.Count; i++)
            {
                var hdd = data[i];
                row = row + i;

                sheet.SetCellAsString(row, HddRetentionColumns.Wg, hdd.Wg);
                sheet.SetCellAsString(row, HddRetentionColumns.WgName, hdd.WgName ?? string.Empty);
                sheet.SetCellAsCurrency(row, HddRetentionColumns.TP, hdd.TransferPrice, currency);
                sheet.SetCellAsCurrency(row, HddRetentionColumns.DealerPrice, hdd.DealerPrice, currency);
                sheet.SetCellAsCurrency(row, HddRetentionColumns.ListPrice, hdd.ListPrice, currency);
            }
        }
    }
}
