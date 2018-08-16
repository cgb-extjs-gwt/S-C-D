using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.CapabilityMatrix
{
    [Table("MatrixAllowView")] //materialized view
    public class CapabilityMatrixAllowView : CapabilityMatrixView
    {
        public bool FujitsuGlobalPortfolio { get; set; }

        public bool MasterPortfolio { get; set; }

        public bool CorePortfolio { get; set; }
    }
}
