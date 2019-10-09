using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.OperationResult;
using Ninject;
using System;

namespace Gdc.Scd.Export.CdCs
{
    public class CdCsJob
    {
        protected ILogger log;

        protected CdCsService cdCs;

        public CdCsJob()
        {
            var Kernel = Module.CreateKernel();
            var Logger = Kernel.Get<ILogger>();
            var repo = Kernel.Get<IRepositorySet>();

            this.log = Logger;
            this.cdCs = new CdCsService(repo, new SharePointClient(Config.NetworkCredential), Logger);
        }

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
                Notify(ex, CdCsMessages.UNEXPECTED_ERROR);
                return Result(false);
            }
        }

        public string WhoAmI()
        {
            return "CdCsJob";
        }

        protected virtual void Notify(Exception ex, string msg)
        {
            Fujitsu.GDC.ErrorNotification.Logger.Error(msg, ex, null, null);
        }

        public OperationResult<bool> Result(bool ok)
        {
            return new OperationResult<bool> { IsSuccess = ok, Result = true };
        }
    }
}
