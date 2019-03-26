namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class HddRetentionDto
    {
        public long WgId { get; set; }

        public string Wg { get; set; }

        public string Sog { get; set; }

        public double? HddRetention { get; set; }

        public double? TransferPrice { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }

        public double? DealerPrice { get; set; }

        public string ChangeUserName { get; set; }

        public string ChangeUserEmail { get; set; }
    }
}
