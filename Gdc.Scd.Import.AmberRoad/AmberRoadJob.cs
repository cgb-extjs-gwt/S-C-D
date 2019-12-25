using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using Ninject;
using System;

namespace Gdc.Scd.Import.AmberRoad
{
    public class AmberRoadJob
    {
        protected ILogger log;

        protected AmberRoadService amber;

        public AmberRoadJob()
        {
            var kernel = Module.CreateKernel();

            this.amber = kernel.Get<AmberRoadService>();
            this.log = kernel.Get<ILogger>();
        }

        protected AmberRoadJob(AmberRoadService amber, ILogger log)
        {
            this.amber = amber;
            this.log = log;
        }

        public OperationResult<bool> Output()
        {
            try
            {
                amber.Run();
                return Result(true);
            }
            catch (Exception ex)
            {
                log.Fatal(ex, ImportConstants.UNEXPECTED_ERROR);
                Notify(ImportConstants.UNEXPECTED_ERROR, ex);
                return Result(false);
            }
        }

        /// <summary>
        /// Method should return job name
        /// which should be similar as "JobName" column in [JobsSchedule] table
        /// </summary>
        /// <returns>Job name</returns>
        public string WhoAmI()
        {
            return "AmberRoadJob";
        }

        public OperationResult<bool> Result(bool ok)
        {
            return new OperationResult<bool> { IsSuccess = ok, Result = true };
        }

        protected virtual void Notify(string msg, Exception ex)
        {
            Fujitsu.GDC.ErrorNotification.Logger.Error(msg, ex, null, null);
        }
    }
}
