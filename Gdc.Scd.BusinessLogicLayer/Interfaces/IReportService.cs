using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        ReportDto[] GetReports();

        ReportSchemaDto GetSchema(long reportId);

        Task<FileStreamDto> Excel(long reportId, ReportFilterCollection filter);

        Task<DataTableDto> GetData(long reportId, ReportFilterCollection filter, int start, int limit);

        Task<JsonArrayDto> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit);
    }
}
