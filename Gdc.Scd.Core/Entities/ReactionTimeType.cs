using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("ReactionTime_ReactionType", Schema = MetaConstants.DependencySchema)]
    public class ReactionTimeType : BaseDisabledEntity
    {
        public ReactionTime ReactionTime { get; set; }

        public ReactionType ReactionType { get; set; }
    }
}
