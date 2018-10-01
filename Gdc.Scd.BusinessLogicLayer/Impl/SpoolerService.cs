using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class SpoolerService : DomainService<JobsSchedule>, ISpoolerService
    {
        public SpoolerService(IRepositorySet repositorySet) : base(repositorySet)
        {
        }
        public JobsSchedule GetJobsScheduleByName(string jobScheduleName)
        {
            return this
                .GetAll()
                .FirstOrDefault(job => job.JobName == jobScheduleName && job.Active);
        }
    }
}
