using System.Configuration;

namespace Gdc.Scd.Export.ArchiveJob
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
