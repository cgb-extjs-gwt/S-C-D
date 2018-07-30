using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix
{
    [Table("CapabilityMatrixDeny")]
    public class CapabilityMatrixDeny : CapabilityMatrix
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }

        [ForeignKey("Id")]
        public CapabilityMatrixAllow CapabilityMatrixAllow { get; set; }
    }
}