using Gdc.Scd.OperationResult;

namespace ExampleJob
{
    public class MyJob
    {
        public OperationResult<bool> Output()
        {
            var result = new OperationResult<bool>
            {
                IsSuccess = true,
                Result = true
            };

            return result;
        }
        /// <summary>
        /// Method should return job name
        /// which should be similar as "JobName" column in [JobsSchedule] table
        /// </summary>
        /// <returns>Job name</returns>
        public string WhoAmI()
        {
            return "ExampleJob";
        }
    }
}
