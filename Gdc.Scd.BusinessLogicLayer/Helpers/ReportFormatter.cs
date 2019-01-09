using System.Globalization;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class ReportFormatter
    {
        static CultureInfo culture = CultureInfo.InvariantCulture;

        public static string Format4Decimals(double v)
        {
            return v == 0 ? "0" : v.ToString("#.####", culture);
        }

        public static string FormatEuro(double v)
        {
            return v.ToString("#.##") + " EUR";
        }

        public static string FormatPercent(double v)
        {
            return v.ToString("#.###") + "%";
        }

        public static string FormatYesNo(bool v)
        {
            return v ? "YES" : "NO";
        }
    }
}
