using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.ClusterRegionInputLevel, Schema = MetaConstants.InputLevelSchema)]
    public class ClusterRegion : NamedId
    {
        public bool IsEmeia { get; set; }
    }
}
