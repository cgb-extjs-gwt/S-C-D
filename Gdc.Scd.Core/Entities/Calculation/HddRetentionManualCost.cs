using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table(MetaConstants.HddRetentionManualCostTable, Schema = MetaConstants.HardwareSchema)]
    public class HddRetentionManualCost : IIdentifiable
    {
        [ForeignKey("Wg")]
        [Column("WgId")]
        public long Id { get; set; }

        public Wg Wg { get; set; }

        //ChangeUserId hack for correct save
        //TODO: remove ChangeUserId
        public long? ChangeUserId { get; set; }
        public User ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public double? TransferPrice { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? DealerPrice { get; private set; }
    }
}
