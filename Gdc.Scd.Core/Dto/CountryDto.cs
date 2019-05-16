namespace Gdc.Scd.Core.Dto
{
    public class CountryDto
    {
        public string CountryName { get; set; }
        public string CountryGroup { get; set; }
        public string Region { get; set; }
        public string LUTCode { get; set; }
        public string CountryDigit { get; set; }
        public string QualityGroup { get; set; }
        public bool CanStoreListAndDealerPrices { get; set; }
        public bool CanOverrideTransferCostAndPrice { get; set; }
        public bool CanOverride2ndLevelSupportLocal { get; set; }
        public bool IsMaster { get; set; }
        public string ISO3Code { get; set; }
        public string Currency { get; set; }
        public long CountryId { get; set; }
    }
}
