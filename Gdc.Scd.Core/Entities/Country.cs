using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

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

        public bool CanOverrideListAndDealerPrices { get; set; }

        public bool ShowDealerPrice { get; set; }

        public bool CanOverrideTransferCostAndPrice { get; set; }
    }
}
