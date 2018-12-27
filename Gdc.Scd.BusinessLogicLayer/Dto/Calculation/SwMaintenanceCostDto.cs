namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class SwMaintenanceCostDto
    {
        public string Sog { get; set; }

        public string Availability { get; set; }

        public string Year { get; set; }

        public double? Reinsurance { get; set; }

        public double? ServiceSupport { get; set; }

        public double? TransferPrice { get; set; }

        public double? MaintenanceListPrice { get; set; }

        public double? DealerPrice { get; set; }
    }
}
