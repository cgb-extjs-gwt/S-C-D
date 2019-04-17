using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.DataAccessLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public class ReportExcelWriter : IDisposable
    {
        private const int DEFAULT_COL_WIDTH = 25;

        private bool prepared;

        private int currentRow;

        private ReportSchemaDto schema;

        private ReportColumnFormat[] fields;

        private IXLWorkbook workbook;

        private IXLWorksheet worksheet;

        public ReportExcelWriter(ReportSchemaDto schema)
        {
            this.schema = schema;
        }

        public Stream GetData()
        {
            var stream = new MemoryStream(3072); //3KB
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

            for (var i = 0; i < fields.Length; i++)
            {
                var f = fields[i];

                if (f.HasValue())
                {
                    f.SetValue(worksheet.Cell(currentRow, i + 1));
                }
            }
        }

        private void WriteHeader()
        {
            for (var i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                if (f.Flex > 1)
                {
                    worksheet.Column(i + 1).Width = f.Flex * DEFAULT_COL_WIDTH;
                }

                var cell = worksheet.Cell(currentRow, i + 1);
                cell.Value = f.Caption;
                cell.Style.Font.Bold = true;
            }
        }

        private void Prepare(DbDataReader reader)
        {
            this.InitWorkbook();
            //
            this.fields = PrepareFields(reader);
            this.WriteHeader();
            //
            this.prepared = true;
        }

        private ReportColumnFormat[] PrepareFields(DbDataReader reader)
        {
            var schemaFields = schema.Fields;
            var len = schemaFields.Length;
            var result = new List<ReportColumnFormat>(len);

            for (int i = 0, k = 0; i < len; i++)
            {
                var f = schemaFields[i];
                if (reader.HasField(f.Name))
                {
                    //ok, column exists in select dataset
                    //add thit column to report
                    //
                    result.Add(new ReportColumnFormat(reader, f, worksheet.Column(k + 1)));
                    k++;
                }
            }

            return result.ToArray();
        }

        private void InitWorkbook()
        {
            this.currentRow = 1;
            this.workbook = new XLWorkbook();

            var sheetName = schema.Name;
            if (sheetName.Length > 31)
            {
                sheetName = sheetName.Substring(0, 31); //ClosedXML limit
            }

            this.worksheet = workbook.Worksheets.Add(sheetName);
            this.worksheet.ColumnWidth = DEFAULT_COL_WIDTH;
        }

        public void Dispose()
        {
            if (this.workbook != null)
            {
                this.workbook.Dispose();
            }
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
                else if (col.IsDate())
                {
                    InitDatetime();
                }
                else
                {
                    InitTxt();
                }
            }

            public int Flex { get { return col.Flex; } }

            public string Caption { get { return col.Text; } }

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

            private void SetDate(IXLCell cell)
            {
                cell.Style.NumberFormat.Format = "yyyy-MM-dd";
                cell.SetValue(reader.GetDateTime(ordinal));
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

            private void InitDatetime()
            {
                fmt = SetDate;
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
