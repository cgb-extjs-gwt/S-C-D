using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities
{
    [Table("AvailabilityFee", Schema = MetaConstants.AtomSchema)]
    public class AvailabilityFee : IIdentifiable, IDeactivatable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("Country")]
        public long? CountryId { get; set; }
        public Country Country { get; set; }

        [Column("Wg")]
        public long? WgId { get; set; }
        public Wg Wg { get; set; }

        public double? InstalledBaseHighAvailability { get; set; }
        public double? TotalLogisticsInfrastructureCost { get; set; }
        public double? StockValueFj { get; set; }
        public double? StockValueMv { get; set; }
        public double? AverageContractDuration { get; set; }
        public double? CostPerKit { get; set; }
        public double? CostPerKitJapanBuy { get; set; }
        public double? MaxQty { get; set; }
        public bool? JapanBuy { get; set; }
        public double? InstalledBaseHighAvailability_Approved { get; set; }
        public double? TotalLogisticsInfrastructureCost_Approved { get; set; }
        public double? StockValueFj_Approved { get; set; }
        public double? StockValueMv_Approved { get; set; }
        public double? AverageContractDuration_Approved { get; set; }
        public bool? JapanBuy_Approved { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
