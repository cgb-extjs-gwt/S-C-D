using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gdc.Scd.Core.Entities
{
    [Table("ClusterRegion", Schema = MetaConstants.InputLevelSchema)]
    public class ClusterRegion : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }
    }
}
