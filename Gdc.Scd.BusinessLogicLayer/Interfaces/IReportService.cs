using System.Collections.Generic;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        IEnumerable<ReportDto> GetReports();
        ReportSchemaDto GetSchema(string type);
    }
}
