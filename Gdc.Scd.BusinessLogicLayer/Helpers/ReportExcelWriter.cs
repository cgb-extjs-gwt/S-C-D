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

        private ReportColumnFormat[] formatters;

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

            var sheetName = schema.Name;
            if(sheetName.Length > 31)
            {
                sheetName = sheetName.Substring(0, 31); //ClosedXML limit
            }

            this.worksheet = workbook.Worksheets.Add(sheetName);
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

        public void WriteBody(DbDataReader reader)
        {
            if (!this.prepared)
            {
                Prepare(reader);
            }

            currentRow++;

            for (var i = 0; i < fieldCount; i++)
            {
                var f = formatters[i];

                if (f.HasValue())
                {
                    worksheet.Cell(currentRow, i + 1).Value = f.format();
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
            formatters = new ReportColumnFormat[fieldCount];

            for (var i = 0; i < fieldCount; i++)
            {
                var f = fields[i];
                formatters[i] = new ReportColumnFormat(reader, f.Name, f.Type);
            }

            this.prepared = true;
        }

        private class ReportColumnFormat
        {
            private readonly DbDataReader reader;

            public int CUR_ORDINAL;
            public readonly int ordinal;

            public readonly Func<object> format;

            public ReportColumnFormat(DbDataReader reader, string fieldName, string type)
            {
                this.reader = reader;
                this.ordinal = reader.GetOrdinal(fieldName);
                this.format = GetFormatter(type);
            }

            public Func<object> GetFormatter(string type)
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
                else if (string.Compare(type, "money", true) == 0)
                {
                    CUR_ORDINAL = reader.GetOrdinal("Currency");
                    return GetMoney;
                }
                else
                {
                    return GetText;
                }
            }

            public bool HasValue()
            {
                return !reader.IsDBNull(ordinal);
            }

            public object GetText()
            {
                return reader[ordinal];
            }

            public object GetNumber()
            {
                return ReportFormatter.Format4Decimals(reader.GetDouble(ordinal));
            }

            public object GetBoolean()
            {
                return ReportFormatter.FormatYesNo(reader.GetBoolean(ordinal));
            }

            public object GetEuro()
            {
                return ReportFormatter.FormatEuro(reader.GetDouble(ordinal));
            }

            public object GetPercent()
            {
                return ReportFormatter.FormatPercent(reader.GetDouble(ordinal));
            }

            public object GetMoney()
            {
                return ReportFormatter.FormatMoney(reader.GetDouble(ordinal), reader.GetString(CUR_ORDINAL));
            }
        }
    }
}
