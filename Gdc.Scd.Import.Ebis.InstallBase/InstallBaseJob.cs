using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Spooler.Core.Entities;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject;
using System;

namespace Gdc.Scd.Import.Ebis.InstallBase
{
    public class InstallBaseJob : IJob
    {
        protected ILogger log;

        protected InstallBaseService installBaseService;

        public InstallBaseJob()
        {
            var kernel = Module.CreateKernel();

            this.log = kernel.Get<ILogger>();
            this.installBaseService = kernel.Get<InstallBaseService>();
        }

        public InstallBaseJob(InstallBaseService installBase, ILogger log)
        {
            this.installBaseService = installBase;
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
                installBaseService.UploadInstallBaseInfo();
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
            return "InstallBaseJob";
        }

        public OperationResult<bool> Result(bool ok)
        {
            return new OperationResult<bool> { IsSuccess = ok, Result = true };
        }

        protected virtual void Notify(string msg, Exception ex)
        {
            Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
        }
    }
}
