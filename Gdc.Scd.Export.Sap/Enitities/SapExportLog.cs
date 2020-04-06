using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Export.Sap.Enitities
{
    public class SapExportLog : IIdentifiable
    {
        public long Id { get; set; }

        public DateTime DateTime { get; set; }

        public ExportType ExportType { get; set; }
    }
}
