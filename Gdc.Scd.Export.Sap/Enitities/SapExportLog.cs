using Gdc.Scd.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Export.Sap.Enitities
{
    [Table("SapExportLogs", Schema = MetaConstants.DefaultSchema)]
    public class SapExportLog : IIdentifiable
    {
        public long Id { get; set; }

        public DateTime UploadDate { get; set; }

        public DateTime? PeriodStartDate { get; set; }

        public int ExportType { get; set; }

        public int FileNumber { get; set; }

        public bool IsSend { get; set; }
    }
}
