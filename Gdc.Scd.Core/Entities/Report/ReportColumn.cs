using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Report
{
    [Table("ReportColumn", Schema = MetaConstants.ReportSchema)]
    public class ReportColumn: NamedId
    {
        public Report Report { get; set; }

        public int Index { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public ReportColumnType Type { get; set; }

        public bool AllowNull { get; set; }

        public int Flex { get; set; }
    }
}