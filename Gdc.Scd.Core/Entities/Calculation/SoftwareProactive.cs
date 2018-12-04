using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table("ProActiveView", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SoftwareProactive : IIdentifiable
    {
        public long Id { get; set; }

        public long? Country { get; set; }
        [ForeignKey("Country")]
        public Country CountryRef { get; set; }

        public long? Sog { get; set; }
        [ForeignKey("Sog")]
        public Sog SogRef { get; set; }

        public long? Year { get; set; }
        [ForeignKey("Year")]
        public Year YearRef { get; set; }

        public double? ProActive { get; set; }
        public double? ProActive_Approved { get; set; }
    }
}
