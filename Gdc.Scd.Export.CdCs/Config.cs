using Gdc.Scd.Export.CdCsJob.Dto;
using System.Configuration;
using System.Net;

namespace Gdc.Scd.Export.CdCsJob
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

        public static NetworkCredential NetworkCredential
        {
            get
            {
                return new NetworkCredential(Config.SpServiceAccount, Config.SpServicePassword, Config.SpServiceDomain);
            }
        }

        public static SpFileDto SpFile
        {
            get
            {
                return new SpFileDto
                {
                    WebUrl = Config.CalculationToolWeb,
                    ListName = Config.CalculationToolList,
                    FolderServerRelativeUrl = Config.CalculationToolFolder,
                    FileName = Config.CalculationToolFileName
                };
            }
        }
    }
}
