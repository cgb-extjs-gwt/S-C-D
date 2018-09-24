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

        Task<(Stream data, string fileName)> Excel(long reportId, ReportFilterCollection filter);

        Task<(DataTable tbl, int total)> GetData(long reportId, ReportFilterCollection filter, int start, int limit);

        Task<(string json, int total)> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit);
    }
}
