namespace Gdc.Scd.Export.CdCs
{
    public static class CdCsMessages
    {       
        public const string START_PROCESS = "CD CS Calculation started...";
        public const string END_PROCESS = "CD CS Calculation has been finished";
        public const string UNEXPECTED_ERROR = "CD CS Calculation completed unsuccessfully. Please find details below.";

        public const string READ_CONFIGURATION = "Reading configuration...";
        public const string READ_TEMPLATE = "Reading CD CS template file...";
        public const string READ_SERVICE = "Reading service costs...";
        public const string WRITE_SERVICE = "Writing service costs...";
        public const string READ_PROACTIVE = "Reading pro active costs...";
        public const string WRITE_PROACTIVE = "Writing pro active costs...";
        public const string READ_HDD_RETENTION = "Reading HDD retention costs...";
        public const string WRITE_HDD_RETENTION = "Writing HDD retention costs...";
        public const string UPLOAD_FILE = "Uploading file...";
        public const string PARSE_SLA_SHEET = "Parsing SLA input sheet...";
        public const string READ_COUNTRY_COSTS = "Reading costs for {0}...";
        public const string WRITE_COUNTRY_COSTS = "Filling CD CS template file for each country...";
    }
}
