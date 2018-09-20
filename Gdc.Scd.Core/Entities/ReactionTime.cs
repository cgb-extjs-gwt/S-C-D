using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("ReactionTime", Schema = MetaConstants.DependencySchema)]
    public class ReactionTime : ExternalEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }
    }
}
