namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class HddRetentionDto
    {
        public long WgId { get; set; }

        public string Wg { get; set; }

        public double? HddRetention { get; set; }

        public double? TransferPrice { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }

        public double? DealerPrice { get; private set; }

        public string ChangeUser { get; set; }

        public string ChangeUserEmail { get; set; }
    }
}
