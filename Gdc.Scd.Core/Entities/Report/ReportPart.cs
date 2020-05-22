using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Report
{
    [Table("ReportPart", Schema = MetaConstants.ReportSchema)]
    public class ReportPart : IIdentifiable
    {
        public long Id { get; set; }

        [Required]
        public Report Report { get; set; }

        [Required]
        public Report Part { get; set; }

        public int Index { get; set; }
    }
}
