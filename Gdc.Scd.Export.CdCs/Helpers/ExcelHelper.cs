using ClosedXML.Excel;

namespace Gdc.Scd.Export.CdCs.Helpers
{
    public static class ExcelHelper
    {
        public static void SetCellAsString(this IXLWorksheet sheet, int row, int column, string value)
        {
            sheet.Cell(row, column).SetValue(value);
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
