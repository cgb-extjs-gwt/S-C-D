using System.Collections.Generic;
using System.Data;
using System.IO;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        IEnumerable<ReportDto> GetReports();

        ReportSchemaDto GetSchema(long reportId);

        Stream Excel(long reportId, ReportFilterCollection filter, out string fileName);

        DataTable GetData(long reportId, ReportFilterCollection filter, int start, int limit, out int total);

        string GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit, out int total);
    }
}
