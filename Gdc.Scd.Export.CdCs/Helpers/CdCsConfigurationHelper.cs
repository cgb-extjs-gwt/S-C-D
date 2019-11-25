using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Export.CdCs.Helpers
{
    public static class CdCsConfigurationHelper
    {
        public static string GetCountry(this CdCsConfiguration c)
        {
            return c.Country.Name;
        }

        public static string GetCurrency(this CdCsConfiguration c)
        {
            return c.Country.Currency.Name;
        }

        public static string GetPriceListPath(this CdCsConfiguration c)
        {
            return string.Concat(
                    c.FileWebUrl,
                    @"/Calculation Output Reporting/Single Calculator (MCT)\..\..\CD_CS_PriceList\",
                    c.GetCountry(),
                    " PriceList_CD_CS.xlsx"
                );
        }
    }
}
