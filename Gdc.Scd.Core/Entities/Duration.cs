using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("Duration", Schema = MetaConstants.DependencySchema)]
    public class Duration : ExternalEntity
    {
        [Key]
        [ForeignKey("Year")]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public Year Year { get; set; }

        public int Value { get; set; }

        public bool IsProlongation { get; set; }
    }
}
