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

        public string ISO2CountryCode { get; set; }

        public string LUT { get; set; }

        public bool CanOverrideListAndDealerPrices { get; set; }

        public bool ShowDealerPrice { get; set; }

        public bool CanOverrideTransferCostAndPrice { get; set; }

        public ClusterRegion ClusterRegion { get; set; }

        public long ClusterRegionId { get; set; }
    }
}
