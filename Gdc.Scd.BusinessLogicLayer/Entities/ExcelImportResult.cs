using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class ExcelImportResult
    {
        public List<string> Errors { get; set; }

        public bool HasErrors => this.Errors != null && this.Errors.Count > 0;

        public QualityGateResult QualityGateResult { get; set; }
    }
}
