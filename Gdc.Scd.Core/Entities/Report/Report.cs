using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Report
{
    [Table("Report", Schema = MetaConstants.ReportSchema)]
    public class Report: NamedId
    {
        [Required]
        public string Title { get; set; }

        public bool CountrySpecific { get; set; }

        public bool HasFreesedVersion { get; set; }

        [Required]
        public string SqlFunc { get; set; }
    }
}
