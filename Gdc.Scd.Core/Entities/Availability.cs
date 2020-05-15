using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("Availability", Schema = MetaConstants.DependencySchema)]
    public class Availability : ExternalEntity
    {
        public int Value { get; set; }
    }
}
