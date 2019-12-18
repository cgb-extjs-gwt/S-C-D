namespace Gdc.Scd.Export.ArchiveResultSender
{
    public static class Config
    {
        public static string MailTo { get; }
        public static string ScdFolder { get; }
        public static string DateFormat { get; }
        public static string MailFrom { get; set; }

        static Config()
        {
            MailTo = System.Configuration.ConfigurationManager.AppSettings["mailTo"];
            ScdFolder = System.Configuration.ConfigurationManager.AppSettings["scdArchiveFolder"];
            DateFormat = System.Configuration.ConfigurationManager.AppSettings["dateFormat"];
            MailFrom = System.Configuration.ConfigurationManager.AppSettings["mailFrom"];
        }
    }
}
