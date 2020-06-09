using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.ProjectCalculator;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class ProjectCalculatorController : BaseDomainController<Project>
    {
        private readonly IProjectCalculatorService projectCalculatorService;

        public ProjectCalculatorController(
            IDomainService<Project> domainService,
            IProjectCalculatorService projectCalculatorService) 
            : base(domainService)
        {
            this.projectCalculatorService = projectCalculatorService;
        }

        public ProjectCalculatorData GetProjectCalculatorData()
        {
            return this.projectCalculatorService.GetProjectCalculatorData();
        }

        public void SaveWithInterpolation(Project[] projects)
        {
            this.SaveWithInterpolation(projects);
        }
    }
}