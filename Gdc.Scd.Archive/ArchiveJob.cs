using Gdc.Scd.OperationResult;

namespace Gdc.Scd.Archive
{
    public class ArchiveJob
    {
        public OperationResult<bool> Output()
        {
            throw new System.NotImplementedException();
            var result = new OperationResult<bool>();
            return result;
        }

        /// <summary>
        /// Method should return job name
        /// which should be similar as "JobName" column in [JobsSchedule] table
        /// </summary>
        /// <returns>Job name</returns>
        public string WhoAmI()
        {
            return "ArchiveJob";
        }
    }
}
