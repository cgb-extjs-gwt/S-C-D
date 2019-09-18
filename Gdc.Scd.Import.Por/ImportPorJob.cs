using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using System;

namespace Gdc.Scd.Import.Por
{
    public class ImportPorJob
    {
        protected ILogger log;

        protected ImportPor importer;

        public ImportPorJob()
        {
            Init();
        }

        public OperationResult<bool> Output()
        {
            try
            {
                importer.Run();
                return Result(true);
            }
            catch (Exception ex)
            {
                log.Fatal(ImportConstantMessages.UNEXPECTED_ERROR, ex);
                Notify(ImportConstantMessages.UNEXPECTED_ERROR, ex);
                return Result(false);
            }
        }

        public string WhoAmI()
        {
            return "PorJob";
        }

        /// <summary>
        /// for testing only
        /// </summary>
        protected virtual void Init()
        {
            log = PorService.ILogger;
            importer = new ImportPor(log);
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
