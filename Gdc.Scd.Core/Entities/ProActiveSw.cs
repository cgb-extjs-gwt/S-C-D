using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    [Table("ProActiveSw", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class ProActiveSw : ICostBlockEntity
    {
        public long Id { get; set; }
        [Column("Country")]
        public string Country { get; set; }

        [Column("Pla")]
        public long? PlaId { get; set; }

        public Pla Pla { get; set; }

        [Column("Sog")]
        public long? SogId { get; set; }

        public Sog Sog { get; set; }

        [Column("SwDigit")]
        public long? SwDigitId { get; set; }

        public SwDigit SwDigit { get; set; }

        [Column("LocalRemoteAccessSetupPreparationEffort")]
        public double? LocalRemoteAccessSetupPreparationEffort { get; set; }

        [Column("LocalRegularUpdateReadyEffort")]
        public double? LocalRegularUpdateReadyEffort { get; set; }

        [Column("LocalPreparationShcEffort")]
        public double? LocalPreparationShcEffort { get; set; }

        [Column("CentralExecutionShcReportCost")]
        public double? CentralExecutionShcReportCost { get; set; }

        [Column("LocalRemoteShcCustomerBriefingEffort")]
        public double? LocalRemoteShcCustomerBriefingEffort { get; set; }

        [Column("LocalOnSiteShcCustomerBriefingEffort")]
        public double? LocalOnSiteShcCustomerBriefingEffort { get; set; }
        [Column("TravellingTime")]
        public double? TravellingTime { get; set; }
        [Column("OnSiteHourlyRate")]
        public double? OnSiteHourlyRate { get; set; }
        [Column("LocalRemoteAccessSetupPreparationEffort_Approved")]
        public double? LocalRemoteAccessSetupPreparationEffort_Approved { get; set; }
        [Column("LocalRegularUpdateReadyEffort_Approved")]
        public double? LocalRegularUpdateReadyEffort_Approved { get; set; }
        [Column("LocalPreparationShcEffort_Approved")]
        public double? LocalPreparationShcEffort_Approved { get; set; }
        [Column("CentralExecutionShcReportCost_Approved")]
        public double? CentralExecutionShcReportCost_Approved { get; set; }
        [Column("LocalRemoteShcCustomerBriefingEffort_Approved")]
        public double? LocalRemoteShcCustomerBriefingEffort_Approved { get; set; }
        [Column("LocalOnSiteShcCustomerBriefingEffort_Approved")]
        public double? LocalOnSiteShcCustomerBriefingEffort_Approved { get; set; }
        [Column("TravellingTime_Approved")]
        public double? TravellingTime_Approved { get; set; }
        [Column("OnSiteHourlyRate_Approved")]
        public double? OnSiteHourlyRate_Approved { get; set; }
        [Column("CreatedDateTime")]
        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
    }
}

