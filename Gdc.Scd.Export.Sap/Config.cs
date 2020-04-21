using System.Configuration;

namespace Gdc.Scd.Export.Sap
{
    public static class Config
    {
        public static string SapFileName => ConfigurationManager.AppSettings["SapFileName"];
        public static string ExportDirectory => ConfigurationManager.AppSettings["ExportDirectory"];
        public static string ExportHost => ConfigurationManager.AppSettings["ExportHost"];
        public static string Admission => ConfigurationManager.AppSettings["Admission"];
    }
}
