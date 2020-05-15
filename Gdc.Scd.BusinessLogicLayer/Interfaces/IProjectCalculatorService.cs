using Gdc.Scd.Core.Entities.ProjectCalculator;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IProjectCalculatorService : IDomainService<Project>
    {
        int GetAvailabilityValue(DayHour start, DayHour end);
    }
}
