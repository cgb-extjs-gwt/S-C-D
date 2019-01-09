using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using System.IO;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        ReportDto[] GetReports();

        ReportSchemaDto GetSchema(long reportId);

        ReportSchemaDto GetSchema(string reportName);

        Task<(Stream data, string fileName)> Excel(long reportId, ReportFilterCollection filter);

        Task<(string json, int total)> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit);
    }
}
