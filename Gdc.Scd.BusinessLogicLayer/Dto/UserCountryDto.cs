namespace Gdc.Scd.BusinessLogicLayer.Dto
{
    public class UserCountryDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsMaster { get; set; }

        public bool CanStoreListAndDealerPrices { get; set; }

        public bool CanOverrideTransferCostAndPrice { get; set; }

        public string ISO3Code { get; set; }
    }
}
