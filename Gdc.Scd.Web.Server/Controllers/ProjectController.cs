using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.Web.Server.Entities;
using Gdc.Scd.Web.Server.Impl;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.ProjectCalculator })]
    public class ProjectController : BaseDomainController<Project>
    {
        private readonly IProjectService projectService;

        public ProjectController(IProjectService projectService) 
            : base(projectService)
        {
            this.projectService = projectService;
        }

        [HttpGet]
        public ProjectItemEditData GetProjectItemEditData()
        {
            return this.projectService.GetProjectItemEditData();
        }

        [HttpGet]
        public ProjectItem[] GetProjectItems(long projectId)
        {
            return this.projectService.GetProjectItems(projectId).ToArray();
        }

        [HttpPost]
        public virtual Project SaveWithInterpolation([FromBody]Project item)
        {
            return this.projectService.SaveWithInterpolation(item);
        }

        [HttpDelete]
        public override void Delete(long id)
        {
            base.Delete(id);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ReportView([FromUri]long id, [FromUri]long projectId, [FromBody]ReportFormData data)
        {
            var reportData =
                await this.projectService.GetReportData(id, projectId, data.AsFilterCollection(), data.Start, data.Limit);

            return this.JsonContent(reportData.Json, reportData.Total);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ReportExport([FromUri]long id, [FromUri]long projectId, [FromBody]ReportFormData data)
        {
            var reportExportData =
                await this.projectService.GetReportExportData(id, projectId, data.AsFilterCollection());

            return this.ExcelContent(reportExportData.Data, reportExportData.FileName);
        }
    }
}