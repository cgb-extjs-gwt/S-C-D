using Gdc.Scd.Core.Entities.ProjectCalculator;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IProjectCalculatorService : IDomainService<Project>
    {
        ProjectCalculatorData GetProjectCalculatorData();

        void SaveWithInterpolation(Project[] projects);
    }
}
