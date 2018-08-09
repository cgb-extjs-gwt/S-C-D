using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix
{
    [Table("MatrixAllowCountryView")] //materialized view
    public class CapabilityMatrixCountryAllowView : CapabilityMatrixView
    {
        public long CountryId { get; set; }
        public string Country { get; set; }
    }
}
