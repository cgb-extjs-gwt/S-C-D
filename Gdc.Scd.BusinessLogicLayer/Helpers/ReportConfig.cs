using System.Configuration;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    internal static class ReportConfig
    {
        public static bool SchemaCache()
        {
            bool result;
            return bool.TryParse(ConfigurationManager.AppSettings["Report.SchemaCache"], out result);
        }
    }
}
