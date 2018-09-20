using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto[]> GetReports();

        Task<ReportSchemaDto> GetSchema(long reportId);

        Stream Excel(long reportId, ReportFilterCollection filter, out string fileName);

        DataTable GetData(long reportId, ReportFilterCollection filter, int start, int limit, out int total);

        string GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit, out int total);
    }
}
