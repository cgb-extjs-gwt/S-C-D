using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table(MetaConstants.CalculationTable, Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SoftwareCalculationResult : IIdentifiable
    {
        public long Id { get; set; }

        #region Dependencies, denormalized

        [ForeignKey("CountryRef")]
        public long CountryId { get; set; }
        [Required]
        public string Country { get; set; }

        [ForeignKey("SogRef")]
        public long SogId { get; set; }
        [Required]
        public string Sog { get; set; }

        [ForeignKey("AvailabilityRef")]
        public long AvailabilityId { get; set; }
        [Required]
        public string Availability { get; set; }

        [ForeignKey("YearRef")]
        public long YearId { get; set; }
        [Required]
        public string Year { get; set; }

        #endregion

        [Required]
        public Country CountryRef { get; set; }

        [Required]
        public Sog SogRef { get; set; }

        [Required]
        public Availability AvailabilityRef { get; set; }

        [Required]
        public Year YearRef { get; set; }

        public double? Reinsurance { get; set; }
        public double? Reinsurance_Approved { get; set; }

        public double? ServiceSupport { get; set; }
        public double? ServiceSupport_Approved { get; set; }

        public double? TransferPrice { get; set; }
        public double? TransferPrice_Approved { get; set; }

        public double? MaintenanceListPrice { get; set; }
        public double? MaintenanceListPrice_Approved { get; set; }

        public double? DealerPrice { get; set; }
        public double? DealerPrice_Approved { get; set; }
    }
}
