using System.Configuration;

namespace Gdc.Scd.Export.Archive
{
    public static class Config
    {
        public static string FilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["FilePath"];
            }
        }
    }
}
