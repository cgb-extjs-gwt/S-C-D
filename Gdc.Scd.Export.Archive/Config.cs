using System.Configuration;

namespace Gdc.Scd.Export.Archive
{
    public static class Config
    {
        public static string SpServiceDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServiceDomain"];
            }
        }
        public static string SpServiceAccount
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServiceAccount"];
            }
        }
        public static string SpServicePassword
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServicePassword"];
            }
        }
        public static string SpServiceHost
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServiceHost"];
            }
        }
        public static string SpServiceFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["SpServiceFolder"];
            }
        }
    }
}
