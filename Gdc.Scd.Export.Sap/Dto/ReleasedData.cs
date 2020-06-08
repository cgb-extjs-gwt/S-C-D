using System;
using System.Linq;
using Gdc.Scd.Export.Sap.Dto;

namespace Gdc.Scd.Export.Sap.Enitities
{
    public class ReleasedData : FieldGetter
    {
        public string SapTable { get; set; }
        public string SapSalesOrg { get; set; }
        public string SapDivision { get; set; }
        public string CurrencyName { get; set; }
        public string FspCodeWg { get; set; }
        public double PriceDb { get; set; }
        public DateTime ValidToDt { get; set; }
        public DateTime ValidFromDt { get; set; }
        public string SapItemCategory { get; set; }
        public SapUploadPackType SapUploadPackType { get; set; }

        public string ValidTo
        {
            get
            {
                var dateStr = ValidToDt.ToString("yyyyMMdd" );
                return dateStr;
            }
        }

        public string ValidFrom
        {
            get
            {
                var dateStr = ValidFromDt.ToString("yyyyMMdd");
                return dateStr;
            }
        }

        public string VariableKey {
            get
            {
                var vkey = string.Empty;

                if (SapTable.Equals("A991") || SapTable.Equals("A975"))
                    vkey =  SapSalesOrg + FspCodeWg;

                else if (SapTable.Equals("A850") || SapTable.Equals("A906"))
                    vkey =  SapSalesOrg + SapDivision + FspCodeWg;

                if (vkey.Length < 100)
                {
                    vkey = vkey.PadRight(100);
                }

                return vkey;
            }
        }

        public string Price
        {
            get
            {
                var val =  PriceDb.ToString("N2");

                if (val.Length < 14)
                {
                    val = val.PadLeft(14);
                }

                return val;
            }

        }

        public string CostCondition
        {
            get
            {
                if (SapUploadPackType == SapUploadPackType.HW)
                {
                    if(Config.HwZBTCArray.Contains(SapItemCategory))
                        return ConditionType.ZBTC.ToString();

                    if (Config.HwZPWAArray.Contains(SapItemCategory))
                        return ConditionType.ZPWA.ToString();

                    return string.Empty;
                }

                //SapUploadPackType.STDW case always ZPWA
                return ConditionType.ZPWA.ToString();
            }
        }
    }
}
