using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using System;
using System.Data.Common;
using System.IO;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public class ReportExcelWriter
    {
        private const int DEFAULT_COL_WIDTH = 25;

        private bool prepared;

        private int currentRow;

        private int[] ordinals;

        private Func<DbDataReader, int, object>[] formats;

        private readonly ReportColumnDto[] fields;

        private readonly int fieldCount;

        private readonly IXLWorkbook workbook;

        private readonly IXLWorksheet worksheet;

        public ReportExcelWriter(ReportSchemaDto schema)
        {
            this.fields = schema.Fields;
            this.fieldCount = fields.Length;

            this.currentRow = 2;
            this.workbook = new XLWorkbook();
            this.worksheet = workbook.Worksheets.Add(schema.Name);
            //
            WriteHeader();
        }

        public Stream GetData()
        {
            var stream = new MemoryStream(3072); //3KB
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public void Write(DbDataReader reader)
        {
            if (!this.prepared)
            {
                Prepare(reader);
            }

            currentRow++;

            for (var i = 0; i < fieldCount; i++)
            {
                var ordinal = ordinals[i];

                if (!reader.IsDBNull(ordinal))
                {
                    worksheet.Cell(currentRow, i + 1).Value = formats[i](reader, ordinal);
                }
            }
        }

        private void WriteHeader()
        {
            worksheet.ColumnWidth = DEFAULT_COL_WIDTH;

            for (var i = 0; i < fieldCount; i++)
            {
                var f = fields[i];

                if (f.Flex > 1)
                {
                    worksheet.Column(i + 1).Width = f.Flex * DEFAULT_COL_WIDTH;
                }

                var cell = worksheet.Cell(currentRow, i + 1);
                cell.Value = f.Text;
                cell.Style.Font.Bold = true;
            }
        }

        private void Prepare(DbDataReader reader)
        {
            ordinals = new int[fieldCount];
            formats = new Func<DbDataReader, int, object>[fieldCount];

            for (var i = 0; i < fieldCount; i++)
            {
                var f = fields[i];
                ordinals[i] = reader.GetOrdinal(f.Name);
                formats[i] = GetFormat(f.Type);
            }

            this.prepared = true;
        }

        public static Func<DbDataReader, int, object> GetFormat(string type)
        {
            if (string.Compare(type, "number", true) == 0)
            {
                return GetNumber;
            }
            else if (string.Compare(type, "boolean", true) == 0)
            {
                return GetBoolean;
            }
            else if (string.Compare(type, "euro", true) == 0)
            {
                return GetEuro;
            }
            else if (string.Compare(type, "percent", true) == 0)
            {
                return GetPercent;
            }
            else
            {
                return GetText;
            }
        }

        public static object GetText(DbDataReader reader, int ordinal)
        {
            return reader[ordinal];
        }

        public static object GetNumber(DbDataReader reader, int ordinal)
        {
            return reader.GetDouble(ordinal).ToString("#.####");
        }

        public static object GetBoolean(DbDataReader reader, int ordinal)
        {
            return reader.GetBoolean(ordinal) ? "YES" : "NO";
        }

        public static object GetEuro(DbDataReader reader, int ordinal)
        {
            return reader.GetDouble(ordinal).ToString("#.##") + " EUR";
        }

        public static object GetPercent(DbDataReader reader, int ordinal)
        {
            return reader.GetDouble(ordinal).ToString("#.##") + "%";
        }
    }
}
