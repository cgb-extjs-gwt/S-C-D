namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class HwCostDto
    {
        public long Id { get; set; }

        public string Fsp { get; set; }

        public string Country { get; set; }

        public string Currency { get; set; }

        public double ExchangeRate { get; set; }

        public string Wg { get; set; }

        public string Sog { get; set; }

        public string Availability { get; set; }

        public string Duration { get; set; }

        public string ReactionType { get; set; }

        public string ReactionTime { get; set; }

        public string ServiceLocation { get; set; }

        public string ProActiveSla { get; set; }

        public int StdWarranty { get; set; }

        public string StdWarrantyLocation { get; set; }

        public double? FieldServiceCost { get; set; }

        public double? ServiceSupportCost { get; set; }

        public double? Logistic { get; set; }

        public double? AvailabilityFee { get; set; }

        public double? HddRetention { get; set; }

        public double? TaxAndDutiesW { get; set; }

        public double? TaxAndDutiesOow { get; set; }

        public double? MaterialW { get; set; }

        public double? MaterialOow { get; set; }

        public double? Reinsurance { get; set; }

        public double? ProActive { get; set; }

        public double? OtherDirect { get; set; }

        public double? LocalServiceStandardWarranty { get; set; }
        public double? LocalServiceStandardWarrantyManual { get; set; }

        public double? Credits { get; set; }

        public double? ServiceTC { get; set; }
        public double? ServiceTCManual { get; set; }

        public double? ServiceTP { get; set; }

        public double? ReActiveTPManual { get; set; }
        public double? ServiceTPManual { get; set; }
        public double? ServiceTP_Released { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }

        public double? DealerPrice { get; set; }
    }
}
