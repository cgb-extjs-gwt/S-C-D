using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using Ninject;
using System;

namespace Gdc.Scd.Import.Ebis.MCiW
{
    public class MCiWJob
    {
        protected MaterialCostService mciw;

        protected ILogger log;

        public MCiWJob()
        {
            var kernel = Module.CreateKernel();

            this.log = kernel.Get<ILogger>();
            this.mciw = kernel.Get<MaterialCostService>();
        }

        protected MCiWJob(
                MaterialCostService mciw,
                ILogger log
            )
        {
            this.mciw = mciw;
            this.log = log;
        }

        public OperationResult<bool> Output()
        {
            try
            {
                mciw.Run();
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
            return "MCiWJob";
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
