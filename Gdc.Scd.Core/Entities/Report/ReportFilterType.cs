using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Report
{
    [Table("ReportFilterType", Schema = MetaConstants.ReportSchema)]
    public class ReportFilterType : NamedId
    {
        public string ExecSql { get; set; }

        public bool MultiSelect { get; set; }

        public bool IsLogin()
        {
            return string.Compare(this.Name, "LOGIN", true) == 0;
        }
    }
}
