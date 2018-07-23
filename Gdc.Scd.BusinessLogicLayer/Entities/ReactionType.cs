using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    [Table("ReactionType", Schema = MetaConstants.DependencySchema)]
    public class ReactionType : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }
    }
}
