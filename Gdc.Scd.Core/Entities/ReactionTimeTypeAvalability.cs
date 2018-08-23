using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("ReactionTime_ReactionType_Avalability", Schema = MetaConstants.DependencySchema)]
    public class ReactionTimeTypeAvalability : IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ReactionTime ReactionTime { get; set; }

        public ReactionType ReactionType { get; set; }

        public Availability Availability { get; set; }
    }
}
