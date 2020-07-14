using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.Core.Entities.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        ReportDto[] GetReports();

        ReportSchemaDto GetSchema(long reportId);

        ReportSchemaDto GetSchema(string reportName);

        Task<ReportExportData> Excel(long reportId, ReportFilterCollection filter, IDictionary<string, object> additionalParams = null);

        Task<ReportExportData> Excel(string reportName, ReportFilterCollection filter, IDictionary<string, object> additionalParams = null);

        Task<ReportData> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit, IDictionary<string, object> additionalParams = null);
    }
}
