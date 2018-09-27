using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table(MetaConstants.CalculationTable, Schema = MetaConstants.HardwareSchema)]
    public class HardwareCalculationResult : IIdentifiable
    {
        public long Id { get; set; }

        #region Dependencies, denormalized

        public long CountryId { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string CountryGroup { get; set; }
        [Required]
        public Country CountryRef { get; set; }

        public long WgId { get; set; }
        [Required]
        public string Wg { get; set; }
        [Required]
        public string WgDescription { get; set; }
        [Required]
        public Wg WgRef { get; set; }

        public long AvailabilityId { get; set; }
        [Required]
        public string Availability { get; set; }
        [Required]
        public Availability AvailabilityRef { get; set; }

        public long DurationId { get; set; }
        [Required]
        public string Duration { get; set; }
        [Required]
        public string DurationValue { get; set; }
        [Required]
        public Duration DurationRef { get; set; }

        public long ReactionTypeId { get; set; }
        [Required]
        public string ReactionType { get; set; }
        [Required]
        public ReactionType ReactionTypeRef { get; set; }

        public long ReactionTimeId { get; set; }
        [Required]
        public string ReactionTime { get; set; }
        [Required]
        public ReactionTime ReactionTimeRef { get; set; }

        public long ServiceLocationId { get; set; }
        [Required]
        public string ServiceLocation { get; set; }
        [Required]
        public ServiceLocation ServiceLocationRef { get; set; }

        #endregion

        #region Calc results

        public double? FieldServiceCost { get; set; }
        public double? FieldServiceCost_Approved { get; set; }

        public double? ServiceSupport { get; set; }
        public double? ServiceSupport_Approved { get; set; }

        public double? Logistic { get; set; }
        public double? Logistic_Approved { get; set; }

        public double? AvailabilityFee { get; set; }
        public double? AvailabilityFee_Approved { get; set; }

        public double? HddRetention { get; set; }
        public double? HddRetention_Approved { get; set; }

        public double? TaxAndDutiesW { get; set; }
        public double? TaxAndDutiesW_Approved { get; set; }

        public double? TaxAndDutiesOow { get; set; }
        public double? TaxAndDutiesOow_Approved { get; set; }

        public double? MaterialW { get; set; }
        public double? MaterialW_Approved { get; set; }

        public double? MaterialOow { get; set; }
        public double? MaterialOow_Approved { get; set; }

        public double? Reinsurance { get; set; }
        public double? Reinsurance_Approved { get; set; }

        public double? ProActive { get; set; }
        public double? ProActive_Approved { get; set; }

        public double? OtherDirect { get; set; }
        public double? OtherDirect_Approved { get; set; }

        public double? LocalServiceStandardWarranty { get; set; }
        public double? LocalServiceStandardWarranty_Approved { get; set; }

        public double? Credits { get; set; }
        public double? Credits_Approved { get; set; }

        public double? ServiceTC { get; set; }
        public double? ServiceTC_Approved { get; set; }
        public double? ServiceTCManual { get; set; }
        public double? ServiceTCManual_Approved { get; set; }

        public double? ServiceTP { get; set; }
        public double? ServiceTP_Approved { get; set; }
        public double? ServiceTPManual { get; set; }
        public double? ServiceTPManual_Approved { get; set; }

        #endregion
    }
}
