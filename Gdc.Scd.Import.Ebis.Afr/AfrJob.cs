using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Spooler.Core.Entities;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject;
using System;

namespace Gdc.Scd.Import.Ebis.Afr
{
    public class AfrJob : IJob
    {
        protected ILogger log;

        protected AfrService afr;

        public AfrJob()
        {
            var kernel = Module.CreateKernel();

            this.log = kernel.Get<ILogger>();
            this.afr = kernel.Get<AfrService>();
        }

        protected AfrJob(AfrService afr, ILogger log)
        {
            this.afr = afr;
            this.log = log;
        }

        IOperationResult IJob.Output()
        {
            return this.Output();
        }

        public OperationResult<bool> Output()
        {
            try
            {
                afr.Run();
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
            return "AfrJob";
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
