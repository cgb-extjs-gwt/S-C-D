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
                    f.SetValue(worksheet.Cell(currentRow, i + 1));
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
                formatters[i] = new ReportColumnFormat(reader, fields[i], worksheet.Column(i + 1));
            }

            this.prepared = true;
        }

        private class ReportColumnFormat
        {
            private DbDataReader reader;

            private ReportColumnDto col;

            private IXLColumn xlCol;

            private int ordinal;

            private int CUR_ORDINAL;

            private Action<IXLCell> fmt;

            public ReportColumnFormat(DbDataReader reader, ReportColumnDto col, IXLColumn xlCol)
            {
                this.reader = reader;
                this.col = col;
                this.xlCol = xlCol;
                this.ordinal = reader.GetOrdinal(col.Name);

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

            public bool HasValue()
            {
                return !reader.IsDBNull(ordinal);
            }

            public void SetValue(IXLCell cell)
            {
                fmt(cell);
            }

            private void SetText(IXLCell cell)
            {
                cell.Value = reader[ordinal];
            }

            private void SetNumber(IXLCell cell)
            {
                cell.SetValue(reader.GetDouble(ordinal));
            }

            private void SetBoolean(IXLCell cell)
            {
                cell.SetValue(ReportFormatter.FormatYesNo(reader.GetBoolean(ordinal)));
            }

            private void SetMoney(IXLCell cell)
            {
                cell.Style.NumberFormat.Format = CurrencyFmt(reader.GetString(CUR_ORDINAL));
                cell.SetValue(reader.GetDouble(ordinal));
            }

            private void InitBoolean()
            {
                fmt = SetBoolean;
            }

            private void InitEuro()
            {
                SetColumnFormat(CurrencyFmt("EUR"));
                fmt = SetNumber;
            }

            private void InitMoney()
            {
                CUR_ORDINAL = reader.GetOrdinal("Currency");
                fmt = SetMoney;
            }

            private void InitNumber()
            {
                SetColumnFormat(@"0.00??");
                fmt = SetNumber;
            }

            private void InitPercent()
            {
                SetColumnFormat(@"0.000\%");
                fmt = SetNumber;
            }

            private void InitTxt()
            {
                fmt = SetText;
            }

            private void SetColumnFormat(string format)
            {
                xlCol.Style.NumberFormat.Format = format;
            }

            private string CurrencyFmt(string cur)
            {
                return string.Concat("0.00\\ [$", cur, "]");
            }
        }
    }
}
