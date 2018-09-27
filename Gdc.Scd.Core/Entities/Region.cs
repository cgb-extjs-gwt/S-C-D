using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.RegionLevel, Schema = MetaConstants.InputLevelSchema)]
    public class Region : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public long ClusterRegionId { get; set; }

        public ClusterRegion ClusterRegion { get; set; }
    }
}
