using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    [Table("SwSpMaintenance", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SwSpMaintenance : IIdentifiable, IDeactivatable
    {
        public long Id { get; set; }

        [Column("Pla")]
        public long? PlaId { get; set; }

        public Pla Pla { get; set; }

        [Column("Sfab")]
        public long? SfabId { get; set; }

        public SFab SFab { get; set; }

        [Column("Sog")]
        public long? SogId { get; set; }

        public Sog Sog { get; set; }

        [Column("SwDigit")]
        public long? SwDigitId { get; set; }

        public SwDigit SwDigit { get; set; }

        [Column("DurationAvailability")]
        public long? DurationAvailabilityId { get; set; }

        [Column("Availability")]
        public long? AvailabilityId { get; set; }

        public Availability Availability { get; set; }

        [Column("2ndLevelSupportCosts")]
        public double? SecondLevelSupportCosts { get; set; }

        [Column("2ndLevelSupportCosts_Approved")]
        public double? SecondLevelSupportCosts_Approved { get; set; }

        public double? InstalledBaseSog { get; set; }

        public double? InstalledBaseSog_Approved { get; set; }

        public double? ReinsuranceFlatfee { get; set; }

        public double? ReinsuranceFlatfee_Approved { get; set; }

        [Column("CurrencyReinsurance")]
        public long? CurrencyReinsuranceId { get; set; }

        public Currency CurrencyReinsurance { get; set; }

        [Column("CurrencyReinsurance_Approved")]
        public long? CurrencyReinsurance_ApprovedId { get; set; }

        public Currency CurrencyReinsurance_Approved { get; set; }

        public double? RecommendedSwSpMaintenanceListPrice { get; set; }

        public double? RecommendedSwSpMaintenanceListPrice_Approved { get; set; }

        public double? MarkupForProductMarginSwLicenseListPrice { get; set; }

        public double? MarkupForProductMarginSwLicenseListPrice_Approved { get; set; }

        public double? ShareSwSpMaintenanceListPrice { get; set; }

        public double? ShareSwSpMaintenanceListPrice_Approved { get; set; }

        public double? DiscountDealerPrice { get; set; }

        public double? DiscountDealerPrice_Approved { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
