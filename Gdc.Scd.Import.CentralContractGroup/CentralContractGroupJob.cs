using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Spooler.Core.Entities;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject;
using System;

namespace Gdc.Scd.Import.CentralContractGroup
{
    public class CentralContractGroupJob : IJob
    {
        protected ILogger log;
        public const string JobName = "CentralContractGroupJob";

        protected CentralContractGroupService contract;

        public CentralContractGroupJob()
        {
            var kernel = Module.CreateKernel();

            this.log = kernel.Get<ILogger>();
            this.contract = kernel.Get<CentralContractGroupService>();
        }

        protected CentralContractGroupJob(
                CentralContractGroupService contract,
                ILogger log
            )
        {
            this.contract = contract;
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
                contract.Run();
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
            return JobName;
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
