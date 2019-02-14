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

            this.currentRow = 1;
            this.workbook = new XLWorkbook();

            var sheetName = schema.Name;
            if (sheetName.Length > 31)
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
                    worksheet.Cell(currentRow, i + 1).Value = f.GetValue();
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
                formatters[i] = new ReportColumnFormat(reader, fields[i]);
            }

            this.prepared = true;
        }

        private class ReportColumnFormat
        {
            private DbDataReader reader;

            private ReportColumnDto col;

            private int ordinal;

            private int CUR_ORDINAL;

            private Func<object> fmt;

            public ReportColumnFormat(DbDataReader reader, ReportColumnDto col)
            {
                this.reader = reader;
                this.col = col;
                this.ordinal = reader.GetOrdinal(col.Name);
                //
                if (col.IsNumber())
                {
                    InitNumber();
                }
                else if (col.IsBoolean())
                {
                    InitBoolean();
                }
                else if (col.IsEuro())
                {
                    InitEuro();
                }
                else if (col.IsPercent())
                {
                    InitPercent();
                }
                else if (col.IsMoney())
                {
                    InitMoney();
                }
                else
                {
                    InitTxt();
                }
            }

            public object GetValue()
            {
                return fmt();
            }

            public bool HasValue()
            {
                return !reader.IsDBNull(ordinal);
            }

            private object GetText()
            {
                return reader[ordinal];
            }

            private object GetNumber()
            {
                return ReportFormatter.Format4Decimals(reader.GetDouble(ordinal));
            }

            private object GetNumberFmt()
            {
                return ReportFormatter.FormatDecimals(reader.GetDouble(ordinal), col.Format);
            }

            private object GetBoolean()
            {
                return ReportFormatter.FormatYesNo(reader.GetBoolean(ordinal));
            }

            private object GetEuro()
            {
                return ReportFormatter.FormatEuro(reader.GetDouble(ordinal));
            }

            private object GetPercent()
            {
                return ReportFormatter.FormatPercent(reader.GetDouble(ordinal));
            }

            private object GetMoney()
            {
                return ReportFormatter.FormatMoney(reader.GetDouble(ordinal), reader.GetString(CUR_ORDINAL));
            }

            private void InitBoolean()
            {
                fmt = GetBoolean;
            }

            private void InitEuro()
            {
                fmt = GetEuro;
            }

            private void InitMoney()
            {
                CUR_ORDINAL = reader.GetOrdinal("Currency");
                fmt = GetMoney;
            }

            private void InitNumber()
            {
                if (string.IsNullOrEmpty(col.Format))
                {
                    fmt = GetNumber;
                }
                else
                {
                    fmt = GetNumberFmt;
                }
            }

            private void InitPercent()
            {
                fmt = GetPercent;
            }

            private void InitTxt()
            {
                fmt = GetText;
            }
        }
    }
}
