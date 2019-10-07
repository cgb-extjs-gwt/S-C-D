using System.Configuration;

namespace Gdc.Scd.Export.CdCs
{
    public static class Config
    {
        public static string SpServiceDomain => ConfigurationManager.AppSettings["SpServiceDomain"];

        public static string SpServiceAccount => ConfigurationManager.AppSettings["SpServiceAccount"];

        public static string SpServicePassword => ConfigurationManager.AppSettings["SpServicePassword"];

        public static string CalculationToolWeb => ConfigurationManager.AppSettings["CalculationToolWeb"];

        public static string CalculationToolList => ConfigurationManager.AppSettings["CalculationToolList"];

        public static string CalculationToolFolder => ConfigurationManager.AppSettings["CalculationToolFolder"];

        public static string CalculationToolFileName => ConfigurationManager.AppSettings["CalculationToolFileName"];

        public static string ProActiveWgList => ConfigurationManager.AppSettings["ProActiveWgList"];
    }
}
