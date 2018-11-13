using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    public static class CdCsMessages
    {       
        public static string START_PROCESS = "CD CS Calculation started...";
        public static string END_PROCESS = "CD CS Calculation has been finished";
        public static string UNEXPECTED_ERROR = "CD CS Calculation completed unsuccessfully. Please find details below.";

        public static string READ_CONFIGURATION = "Reading configuration...";
        public static string READ_TEMPLATE = "Reading CD CS template file...";
        public static string READ_SERVICE = "Reading service costs...";
        public static string WRITE_SERVICE = "Writing service costs...";
        public static string READ_PROACTIVE = "Reading pro active costs...";
        public static string WRITE_PROACTIVE = "Writing pro active costs...";
        public static string READ_HDD_RETENTION = "Reading global HDD retention costs...";
        public static string WRITE_HDD_RETENTION = "Writing HDD retention costs...";
        public static string UPLOAD_FILE = "Uploading file...";
        public static string READ_SLA_FILE = "Reading SLA input file...";
        public static string PARSE_SLA_FILE = "Parsing SLA input file...";
        public static string READ_COUNTRY_COSTS = "Reading costs for {0}...";
        public static string WRITE_COUNTRY_COSTS = "Filling CD CS template file for each country...";
    }
}
