using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Report
{
    [Table("ReportColumnType", Schema = MetaConstants.ReportSchema)]
    public class ReportColumnType : NamedId { }
}