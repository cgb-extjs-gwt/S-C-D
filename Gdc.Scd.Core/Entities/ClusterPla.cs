using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("ClusterPla", Schema = MetaConstants.InputLevelSchema)]
    public class ClusterPla : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public List<Pla> Plas { get; set; }
    }
}
