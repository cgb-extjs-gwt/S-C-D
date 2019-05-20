using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Internal;
using System.Configuration;

namespace Gdc.Scd.Export.ArchiveResultSender
{
    public static class Config
    {
        public static string MailTo { get; }
        public static  string ScdFolder { get; }
        public static string DateFormat { get; }

        static Config()
        {
            MailTo = System.Configuration.ConfigurationManager.AppSettings["mailTo"];
            ScdFolder = System.Configuration.ConfigurationManager.AppSettings["scdArchiveFolder"];
            DateFormat = System.Configuration.ConfigurationManager.AppSettings["dateFormat"];
        }
    }
}
