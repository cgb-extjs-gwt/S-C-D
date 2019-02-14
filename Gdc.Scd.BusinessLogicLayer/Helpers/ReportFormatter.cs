using System.Globalization;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class ReportFormatter
    {
        static CultureInfo culture = CultureInfo.InvariantCulture;

        public static string Format4Decimals(double v)
        {
            return FormatDecimals(v, "0.####");
        }

        public static string FormatDecimals(double v, string fmt)
        {
            return v.ToString(fmt, culture);
        }

        public static string FormatEuro(double v)
        {
            return v.ToString("0.00", culture) + " EUR";
        }

        public static string FormatMoney(double v, string currency)
        {
            return string.Concat(v.ToString("0.00", culture), " ", currency);
        }

        public static string FormatPercent(double v)
        {
            return v.ToString("0.000", culture) + "%";
        }

        public static string FormatYesNo(bool v)
        {
            return v ? "YES" : "NO";
        }
    }
}
