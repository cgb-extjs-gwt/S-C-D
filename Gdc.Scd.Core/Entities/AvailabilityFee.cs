using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("AvailabilityFee", Schema = MetaConstants.HardwareSchema)]
    public class AvailabilityFee : IIdentifiable, IDeactivatable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column(MetaConstants.CountryInputLevelName)]
        public long? CountryId { get; set; }
        public Country Country { get; set; }

        [Column(MetaConstants.WgInputLevelName)]
        public long? WgId { get; set; }
        public Wg Wg { get; set; }

        public double? InstalledBaseHighAvailability { get; set; }
        public double? TotalLogisticsInfrastructureCost { get; set; }
        public double? StockValueFj { get; set; }
        public double? StockValueMv { get; set; }
        public double? AverageContractDuration { get; set; }
        public double? CostPerKit { get; set; }
        public double? CostPerKit_Approved { get; set; }
        public double? CostPerKitJapanBuy { get; set; }
        public double? CostPerKitJapanBuy_Approved { get; set; }
        public double? MaxQty { get; set; }
        public double? MaxQty_Approved { get; set; }
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
