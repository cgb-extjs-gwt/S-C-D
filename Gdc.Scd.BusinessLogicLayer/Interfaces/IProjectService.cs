using Gdc.Scd.Core.Entities.ProjectCalculator;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IProjectService : IDomainService<Project>
    {
        IQueryable<ProjectItem> GetProjectItems(long projectId);

        ProjectItemEditData GetProjectItemEditData();

        Project SaveWithInterpolation(Project item);
    }
}
