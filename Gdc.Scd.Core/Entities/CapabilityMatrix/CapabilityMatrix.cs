using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.CapabilityMatrix
{
    [Table("Matrix", Schema = MetaConstants.MatrixSchema)]
    public class CapabilityMatrix : CapabilityMatrixSla
    {
        [Required]
        public Country Country { get; set; }
    }
}