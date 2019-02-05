using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("Duration_Availability", Schema = MetaConstants.DependencySchema)]
    public class DurationAvailability : BaseDisabledEntity
    {
        public Duration Year { get; set; }

        public Availability Availability { get; set; }
    }
}
