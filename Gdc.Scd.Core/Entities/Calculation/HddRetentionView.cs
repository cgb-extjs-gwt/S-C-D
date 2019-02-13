using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table("HddRetentionView", Schema = MetaConstants.HardwareSchema)]
    public class HddRetentionView : IIdentifiable
    {
        [Column("WgId")]
        public long Id { get; set; }

        public long WgId { get; set; }

        public string Wg { get; set; }

        public double? HddRet { get; set; }

        public double? HddRet_Approved { get; set; }

        public double? TransferPrice { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }

        public double? DealerPrice { get; private set; }

        public string ChangeUserName { get; set; }

        public string ChangeUserEmail { get; set; }
    }
}
