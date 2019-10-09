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
    }
}
