using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table(MetaConstants.ManualCostTable, Schema = MetaConstants.HardwareSchema)]
    public class HardwareManualCost : IIdentifiable
    {
        [ForeignKey("Matrix")]
        [Column("MatrixId")]
        public long Id { get; set; }

        public CapabilityMatrix.CapabilityMatrix Matrix { get; set; }

        public double? ServiceTC { get; set; }
        public double? ServiceTC_Approved { get; set; }

        public double? ServiceTP { get; set; }
        public double? ServiceTP_Approved { get; set; }

        public double? ListPrice { get; set; }
        public double? ListPrice_Approved { get; set; }

        public double? DealerDiscount { get; set; }
        public double? DealerDiscount_Approved { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? DealerPrice { get; private set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? DealerPrice_Approved { get; private set; }
    }
}
