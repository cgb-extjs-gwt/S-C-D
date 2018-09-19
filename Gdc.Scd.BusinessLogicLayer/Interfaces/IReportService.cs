using System.Collections.Generic;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        IEnumerable<ReportDto> GetReports();

        ReportSchema GetSchema(string type);

        object Excel(string type);

        IEnumerable<object> GetData(string type, ReportFilterCollection filter, int start, int limit, out int total);
    }
}
