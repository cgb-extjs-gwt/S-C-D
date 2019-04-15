using ClosedXML.Excel;
using System;
using System.Data.Common;
using System.IO;

namespace Gdc.Scd.Export.Archive
{
    public class ExcelWriter : IDisposable
    {
        private const int DEFAULT_COL_WIDTH = 25;

        private bool prepared;

        private int currentRow;

        private readonly IXLWorkbook workbook;

        private readonly IXLWorksheet worksheet;

        public ExcelWriter(string sheetName)
        {
            this.currentRow = 1;
            this.workbook = new XLWorkbook();

            if (sheetName.Length > 31)
            {
                sheetName = sheetName.Substring(0, 31); //ClosedXML limit
            }

            this.worksheet = workbook.Worksheets.Add(sheetName);
        }

        public Stream GetData()
        {
            var stream = new MemoryStream(1028 * 128); //128KB
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            Dispose();
            return stream;
        }

        public void WriteBody(DbDataReader reader)
        {
            if (!this.prepared)
            {
                Prepare(reader);
            }

            currentRow++;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (!reader.IsDBNull(i))
                {
                    var cell = worksheet.Cell(currentRow, i + 1);
                    cell.Value = reader[i];
                }
            }
        }

        public void WriteHeader(DbDataReader reader)
        {
            worksheet.ColumnWidth = DEFAULT_COL_WIDTH;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 1);
                cell.Value = reader.GetName(i);
                cell.Style.Font.Bold = true;
            }
        }

        private void Prepare(DbDataReader reader)
        {
            WriteHeader(reader);
            this.prepared = true;
        }

        public void Dispose()
        {
            if (this.workbook != null)
            {
                this.workbook.Dispose();
            }
        }
    }
}
