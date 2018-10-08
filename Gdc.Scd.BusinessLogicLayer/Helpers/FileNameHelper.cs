using System.Text.RegularExpressions;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class FileNameHelper
    {
        private static readonly Regex REPORT_FN_REGEX = new Regex(@"[\s\W]+", RegexOptions.Compiled);

        public static string Excel(string fn)
        {
            fn = REPORT_FN_REGEX.Replace(fn, "-");
            return string.Concat(fn, "-", DateHelper.Timestamp(), ".xlsx");
        }
    }
}
