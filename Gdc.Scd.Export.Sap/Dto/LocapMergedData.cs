using System;

namespace Gdc.Scd.Export.Sap.Dto
{
    public class LocapMergedData
    {
        public long PortfolioId { get; set; }
        public string Currency { get; set; }
        public int CountryId { get; set; }
        public string FspCode { get; set; }
        public double ServiceTP { get; set; }
        public string WgName { get; set; }
        public double LocalServiceStdw { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? NextSapUploadDate { get; set; }
        public string SapItemCategory { get; set; }
        public string SapSalesOrganization { get; set; }
        public string SapDivision { get; set; }
    }
}
