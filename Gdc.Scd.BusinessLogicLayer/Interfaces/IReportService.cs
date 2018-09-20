using System.Collections.Generic;
using System.Data;
using System.IO;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        IEnumerable<ReportDto> GetReports();

        ReportSchema GetSchema(string type);

        Stream Excel(string type);

        DataTable GetData(string type, ReportFilterCollection filter, int start, int limit, out int total);

        string GetJsonArrayData(string type, ReportFilterCollection filter, int start, int limit, out int total);
    }
}
