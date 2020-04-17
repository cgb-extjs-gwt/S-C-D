using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.AvailabilityFeeWgCostBlock, Schema = MetaConstants.HardwareSchema)]
    public class AvailabilityFeeWg : BaseCostBlock, IModifiable
    {
        [Column(MetaConstants.WgInputLevelName)]
        public long? WgId { get; set; }

        public Wg Wg { get; set; }

        public double? CostPerKit { get; set; }

        public double? CostPerKitJapanBuy { get; set; }

        public double? MaxQty { get; set; }

        public DateTime ModifiedDateTime { get; set; }
    }
}
