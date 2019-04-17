namespace Gdc.Scd.Export.CdCs.Enums
{
    public class Enums
    {

        public static class InputMctCdCsWGsColumns
        {
            public static readonly int ServiceLocation = 1;
            public static readonly int Availability = 2;
            public static readonly int ReactionTime = 3;
            public static readonly int ReactionType = 4;
            public static readonly int WarrantyGroup = 5;
            public static readonly int Duration = 6;
            public static readonly int ServiceTC = 7;
            public static readonly int ServiceTP = 8;
            public static readonly int ServiceTP_MonthlyYear1 = 9;
            public static readonly int ServiceTP_MonthlyYear2 = 10;
            public static readonly int ServiceTP_MonthlyYear3 = 11;
            public static readonly int ServiceTP_MonthlyYear4 = 12;
            public static readonly int ServiceTP_MonthlyYear5 = 13;
        }

        public static class ProActiveOutputColumns
        {
            public static readonly int Wg = 1;
            public static readonly int ProActive6 = 2;
            public static readonly int ProActive7 = 3;
            public static readonly int ProActive3 = 4;
            public static readonly int ProActive4 = 5;
            public static readonly int OneTimeTask = 6;
        }

        public static class HddRetentionColumns
        {
            public static readonly int Wg = 1;
            public static readonly int WgName = 2;
            public static readonly int TP = 3;
            public static readonly int DealerPrice = 4;
            public static readonly int ListPrice = 5;
        }

        public static class InputSheets
        {
            public static readonly string InputMctCdCsWGs = "Input_MCT_CD_CS_WGs";
            public static readonly string ProActiveOutput = "ProActive_SCD_Output";
            public static readonly string HddRetention = "HDD_Retention";          
        }

        public static class Functions
        {
            public static readonly string GetServiceCostsBySla = "Report.GetServiceCostsBySla";
            public static readonly string GetProActiveByCountryAndWg = "Report.GetProActiveByCountryAndWg";
            public static readonly string HddRetention = "Report.HddRetention";
        }
        
    }
}
