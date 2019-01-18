using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class ExcelImportResult
    {
        public List<string> Errors { get; set; }

        public bool HasErrors => this.Errors != null && this.Errors.Count > 0;
    }
}
