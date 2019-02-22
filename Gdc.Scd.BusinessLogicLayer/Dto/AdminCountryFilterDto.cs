namespace Gdc.Scd.BusinessLogicLayer.Dto
{
    public class AdminCountryFilterDto
    {
        public string Country { get; set; }
        public long? Group {get;set;}
        public long? Region { get; set; }

        public string Lut {get;set;}
        public string Digit {get;set;}
        public string Iso {get;set;}
        public string QualityGroup { get; set; }

        public bool? IsMaster {get;set;}
        public bool? StoreListAndDealer { get; set; }
        public bool? OverrideTCandTP { get; set; }
}
}
