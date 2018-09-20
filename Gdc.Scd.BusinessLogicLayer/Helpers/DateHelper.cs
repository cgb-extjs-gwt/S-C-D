using System;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class DateHelper
    {
        private static string[] nums = new string[] {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59"
        };

        public static string Timestamp()
        {
            var now = DateTime.Now;
            return Timestamp(ref now);
        }

        public static string Timestamp(ref DateTime d)
        {
            return string.Concat(YearStr(d.Year), nums[d.Month], nums[d.Day], nums[d.Hour], nums[d.Minute], nums[d.Second]);
        }

        private static string YearStr(int year)
        {
            return year.ToString().PadLeft(4);
        }
    }
}