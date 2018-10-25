using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.CapabilityMatrix
{
    [Table("MatrixMaster", Schema = MetaConstants.MatrixSchema)]
    public class CapabilityMatrixMaster : CapabilityMatrixSla
    {
        public bool FujitsuGlobalPortfolio { get; set; }

        public bool MasterPortfolio { get; set; }

        public bool CorePortfolio { get; set; }
    }
}