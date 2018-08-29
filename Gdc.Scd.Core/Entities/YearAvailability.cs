using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("Year_Availability", Schema = MetaConstants.DependencySchema)]
    public class YearAvailability : IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public Year Year { get; set; }

        public Availability Availability { get; set; }
    }
}
