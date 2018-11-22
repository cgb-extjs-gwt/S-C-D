using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table("SwSpMaintenanceCostView", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SoftwareMaintenance : IIdentifiable
    {
        public long Id { get; set; }

        public long? Sog { get; set; }
        [ForeignKey("Sog")]
        public Sog SogRef { get; set; }

        public long? Availability { get; set; }
        [ForeignKey("Availability")]
        public Availability AvailabilityRef { get; set; }

        public long? Year { get; set; }
        [ForeignKey("Year")]
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
