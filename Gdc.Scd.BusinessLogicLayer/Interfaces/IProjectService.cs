using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.Core.Entities.Report;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IProjectService : IDomainService<Project>
    {
        IQueryable<ProjectItem> GetProjectItems(long projectId);

        ProjectItemEditData GetProjectItemEditData();

        Project SaveWithInterpolation(Project item);

        Task<ReportData> GetReportData(long reportId, long projectId, ReportFilterCollection filter, int start, int limit);

        Task<ReportExportData> GetReportExportData(long reportId, long projectId, ReportFilterCollection filter);
    }
}
