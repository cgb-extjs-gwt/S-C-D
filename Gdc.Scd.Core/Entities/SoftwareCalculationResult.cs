using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CalculationTable, Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SoftwareCalculationResult : IIdentifiable
    {
        public long Id { get; set; }

        [Required]
        public Country Country { get; set; }

        [Required]
        public Sog Sog { get; set; }

        [Required]
        public Year Year { get; set; }

        [Required]
        public Availability Availability { get; set; }

        public double? Reinsurance { get; set; }

        public double? ServiceSupport { get; set; }

        public double? TransferPrice { get; set; }

        public double? MaintenanceListPrice { get; set; }

        public double? DealerPrice { get; set; }
    }
}
