using Gdc.Scd.Import.Por;
using System;
using System.Configuration;
using System.Linq;
using Gdc.Scd.Export.Sap.Dto;

namespace Gdc.Scd.Export.Sap
{
    public class Config : FieldGetter
    {
        public static string SapFileName => ConfigurationManager.AppSettings["SapFileName"];
        public static string ExportDirectory => ConfigurationManager.AppSettings["ExportDirectory"];
        public static string ExportHost => ConfigurationManager.AppSettings["ExportHost"];
        public static string Admission => ConfigurationManager.AppSettings["Admission"];
        public static string ExportType => ConfigurationManager.AppSettings["ExportType"];

        public string BiSessionName => ConfigurationManager.AppSettings["BiSessionName"];
        public string Client => ConfigurationManager.AppSettings["Client"];
        public string CpicUser => ConfigurationManager.AppSettings["CpicUser"];
        public string SapTransactionName => ConfigurationManager.AppSettings["SapTransactionName"];
        public string App => ConfigurationManager.AppSettings["App"];
        public string PriceUnit => ConfigurationManager.AppSettings["PriceUnit"];

        public static string[] HwZBTCArray => ConfigurationManager.AppSettings["HwZBTClist"].Split(',');
        public static string[] HwZPWAArray => ConfigurationManager.AppSettings["HwZPWAlist"].Split(',');

        public static string FileHeader => ConfigurationManager.AppSettings["FileHeader"];
        public static string FileLine1 => ConfigurationManager.AppSettings["FileLine1"];
        public static string FileLine2 => ConfigurationManager.AppSettings["FileLine2"];

        public static string SapDBUserLogin => ConfigurationManager.AppSettings["SapDBUser"];

        public static string RegExpConfig => ConfigurationManager.AppSettings["RegExpConfig"];
        public static string RegExpClass => ConfigurationManager.AppSettings["RegExpClass"];
    }
}
