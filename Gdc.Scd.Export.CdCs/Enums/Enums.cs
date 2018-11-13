using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    public class Enums
    {
        public static class InputFileCoumns
        {
            public static readonly int FspCode = 1;
            public static readonly int ServiceLocation = 3;
            public static readonly int Availability = 4;
            public static readonly int ReactionTime = 5;
            public static readonly int ReactionType = 6;
            public static readonly int WarrantyGroup = 7;
            public static readonly int Duration = 8;
        }

        public static class InputMctCdCsWGsColumns
        {
            public static readonly int CountryGroup = 1;
            public static readonly int FspCode = 2;
            public static readonly int ServiceTC = 4;
            public static readonly int ServiceTP = 5;
            public static readonly int ServiceTP_MonthlyYear1 = 6;
            public static readonly int ServiceTP_MonthlyYear2 = 7;
            public static readonly int ServiceTP_MonthlyYear3 = 8;
            public static readonly int ServiceTP_MonthlyYear4 = 9;
            public static readonly int ServiceTP_MonthlyYear5 = 10;
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
            public static readonly string CalculationToolInput = "CD_CS Calculation tool input";
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
