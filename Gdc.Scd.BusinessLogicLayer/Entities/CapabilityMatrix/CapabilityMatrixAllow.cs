using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix
{
    [Table("CapabilityMatrixAllow")]
    public class CapabilityMatrixAllow : CapabilityMatrix
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
    }
}
