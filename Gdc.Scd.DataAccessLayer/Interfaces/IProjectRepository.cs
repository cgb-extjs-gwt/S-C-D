using Gdc.Scd.Core.Entities.ProjectCalculator;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        IQueryable<ProjectItem> GetProjectItems(long projectId);

        void Interpolate(long[] projectItemIds);
    }
}
