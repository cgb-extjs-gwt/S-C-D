using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.CapabilityMatrix
{
    [Table("MatrixMaster", Schema = MetaConstants.MatrixSchema)]
    public class CapabilityMatrixMaster : CapabilityMatrixSla
    {
        public bool DeniedFujitsu { get; set; }

        public bool DeniedMaster { get; set; }

        public bool DeniedCore { get; set; }
    }
}