using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using System;

namespace Gdc.Scd.Export.CdCs
{
    public class CdCsJob
    {
        protected ILogger log;

        protected CdCsService cdCs;

        public CdCsJob() { }

        protected CdCsJob(CdCsService cdCs, ILogger log)
        {
            this.cdCs = cdCs;
            this.log = log;
        }

        public OperationResult<bool> Output()
        {
            try
            {
                cdCs.Run();
                return Result(true);
            }
            catch (Exception ex)
            {
                log.Fatal(ex, CdCsMessages.UNEXPECTED_ERROR);
                Notify(CdCsMessages.UNEXPECTED_ERROR, ex);
                return Result(false);
            }
        }

        public string WhoAmI()
        {
            return "CdCsJob";
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
