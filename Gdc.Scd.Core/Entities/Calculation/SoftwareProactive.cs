using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table("ProActiveView", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SoftwareProactive : IIdentifiable
    {
        public long Id { get; set; }

        public Country Country { get; set; }

        public Sog Sog { get; set; }

        public SwDigit SwDigit { get; set; }

        public Year Year { get; set; }

        public double? ProActive { get; set; }
        public double? ProActive_Approved { get; set; }
    }
}
