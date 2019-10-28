using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using Ninject;
using System;

namespace Gdc.Scd.Import.Logistics
{
    public class LogisticsJob
    {
        protected ILogger log;

        protected LogisticsImportService logistic;

        public LogisticsJob()
        {
            NinjectExt.IsConsoleApplication = true;
            var kernel = Module.CreateKernel();
            //
            log = kernel.Get<ILogger>();
            logistic = kernel.Get<LogisticsImportService>();
        }

        /// <summary>
        /// for testing only
        /// </summary>
        protected LogisticsJob(LogisticsImportService logistic, ILogger log)
        {
            this.logistic = logistic;
            this.log = log;
        }

        public OperationResult<bool> Output()
        {
            try
            {
                logistic.Run();
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
            return "LogisticsImportJob";
        }

        protected virtual void Notify(string msg, Exception ex)
        {
            Fujitsu.GDC.ErrorNotification.Logger.Error(msg, ex, null, null);
        }

        public OperationResult<bool> Result(bool ok)
        {
            return new OperationResult<bool> { IsSuccess = ok, Result = true };
        }
    }
}
