namespace Gdc.Scd.Export.CdCs.Dto
{
    public class ServiceCostDto
    {
        public string Key { get; set; }
        public string CountryGroup { get; set; }

        public string ServiceLocation { get; set; }
        public string Availability { get; set; }
        public string ReactionTime { get; set; }
        public string ReactionType { get; set; }
        public string WarrantyGroup { get; set; }
        public string Duration { get; set; }

        public double ServiceTC { get; set; }
        public double ServiceTP { get; set; }
        public double ServiceTP_MonthlyYear1 { get; set; }
        public double ServiceTP_MonthlyYear2 { get; set; }
        public double ServiceTP_MonthlyYear3 { get; set; }
        public double ServiceTP_MonthlyYear4 { get; set; }
        public double ServiceTP_MonthlyYear5 { get; set; }
    }
}
