using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ISpoolerService : IDomainService<JobsSchedule>
    {
        JobsSchedule GetJobsScheduleByName(string jobScheduleName);
    }
}
