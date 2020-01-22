using ClosedXML.Excel;

namespace Gdc.Scd.Export.CdCsJob.Helpers
{
    public static class ExcelHelper
    {
        public static void ClearFrom(this IXLWorksheet sheet, int startRow)
        {
            var range = sheet.RangeUsed();

            for (int i = startRow; i <= range.RowCount(); i++)
            {
                range.Row(i).Clear();
            }
        }

        public static IXLCell SetCellAsString(this IXLWorksheet sheet, int row, int column, string value)
        {
            return sheet.Cell(row, column).SetValue(value);
        }

        public static void SetCellAsDouble(this IXLWorksheet sheet, int row, int column, double value)
        {
            sheet.Cell(row, column).SetValue(value);
            sheet.Cell(row, column).Style.NumberFormat.Format = "0.0000"; ;
        }

        public static void SetCellAsCurrency(this IXLWorksheet sheet, int row, int column, double value, string cur)
        {
            sheet.Cell(row, column).SetValue(value);
            sheet.Cell(row, column).Style.NumberFormat.Format = $"0.00\\ [${cur}]";
        }
    }
}
