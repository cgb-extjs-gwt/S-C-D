using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("ReactionTime", Schema = MetaConstants.DependencySchema)]
    public class ReactionTime : ExternalEntity
    {
        public int Minutes { get; set; }
    }
}
