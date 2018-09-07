namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class SwCostDto
    {
        public string Country { get; set; }

        public string Sog { get; set; }

        public string Availability { get; set; }

        public string Year { get; set; }

        public double? Reinsurance { get; set; }
        public double? Reinsurance_Approved { get; set; }

        public double? ServiceSupport { get; set; }
        public double? ServiceSupport_Approved { get; set; }

        public double? TransferPrice { get; set; }
        public double? TransferPrice_Approved { get; set; }

        public double? MaintenanceListPrice { get; set; }
        public double? MaintenanceListPrice_Approved { get; set; }

        public double? DealerPrice { get; set; }
        public double? DealerPrice_Approved { get; set; }
    }
}
