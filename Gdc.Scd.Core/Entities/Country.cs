using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CountryInputLevelName, Schema = MetaConstants.InputLevelSchema)]
    public class Country : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string ISO3CountryCode { get; set; }

        public string SAPCountryCode { get; set; }

        public bool CanStoreListAndDealerPrices { get; set; }

        public bool CanOverrideTransferCostAndPrice { get; set; }

        public bool IsMaster { get; set; }

        public string QualityGateGroup { get; set; }

        public CountryGroup CountryGroup { get; set; }

        public long CountryGroupId { get; set; }
    }
}
