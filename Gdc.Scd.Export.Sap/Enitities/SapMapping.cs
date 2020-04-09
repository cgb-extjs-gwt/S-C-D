using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Export.Sap.Enitities
{
    public class SapMapping : IIdentifiable
    {
        public long Id { get; set; }

        public Country Country { get; set; }

        public string SapCountryName { get; set; }

        public string SapDivision { get; set; }

        public string SapCountryCode { get; set; }

        public string SapSalesOrganization { get; set; }
    }
}
