using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.Web.Server.Impl;
using System.Linq;
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
    }
}