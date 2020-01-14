using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using Ninject;
using System;

namespace Gdc.Scd.Export.ArchiveResultSenderJob
{
    public class ArchiveResultSenderJob
    {
        protected ArchiveResultService archive;

        protected ILogger log;

        public ArchiveResultSenderJob()
        {
            var kernel = Module.CreateKernel();

            this.log = kernel.Get<ILogger>();
            this.archive = kernel.Get<ArchiveResultService>();
        }

        protected ArchiveResultSenderJob(ArchiveResultService archive, ILogger log)
        {
            this.archive = archive;
            this.log = log;
        }

        public OperationResult<bool> Output()
        {
            try
            {
                archive.Run();
                return Result(true);
            }
            catch (Exception ex)
            {
                log.Fatal(ex, "Unexpected error occured");
                Notify("Unexpected error occured", ex);
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
            return "ArchiveResultSenderJob";
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
