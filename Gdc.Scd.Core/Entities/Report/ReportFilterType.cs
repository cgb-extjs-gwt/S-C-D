using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Report
{
    [Table("ReportFilterType", Schema = MetaConstants.ReportSchema)]
    public class ReportFilterType : NamedId { }
}
