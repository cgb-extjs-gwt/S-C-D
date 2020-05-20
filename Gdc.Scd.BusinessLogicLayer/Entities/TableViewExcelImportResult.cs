using Gdc.Scd.Core.Entities.QualityGate;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class TableViewExcelImportResult
    {
        public List<string> Errors { get; set; }

        public QualityGateResultSet QualityGateResult { get; set; }
    }
}
