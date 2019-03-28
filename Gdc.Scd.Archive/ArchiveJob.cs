using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.OperationResult;
using Ninject;
using System;

namespace Gdc.Scd.Archive
{
    public class ArchiveJob
    {
        protected ArchiveService srv;

        protected ILogger logger;

        public ArchiveJob()
        {
            Init();
        }

        public OperationResult<bool> Output()
        {
            try
            {
                srv.Run();
                return Result(true);
            }
            catch (Exception e)
            {
                logger.Log(ScdLogLevel.Fatal, e, ArchiveConstants.UNEXPECTED_ERROR);
                Notify(ArchiveConstants.UNEXPECTED_ERROR, e);
                return Result(false);
            }
        }

        public string WhoAmI()
        {
            return "ArchiveJob";
        }

        /// <summary>
        /// for testing only
        /// </summary>
        protected virtual void Init()
        {
            var kernel = Module.CreateKernel();
            //
            logger = kernel.Get<ILogger>();
            srv = kernel.Get<ArchiveService>();
        }

        /// <summary>
        /// for testing only
        /// </summary>
        protected virtual void Notify(string msg, Exception e)
        {
            Fujitsu.GDC.ErrorNotification.Logger.Error(msg, e, null, null);
        }

        public OperationResult<bool> Result(bool ok)
        {
            return new OperationResult<bool> { IsSuccess = ok, Result = true };
        }
    }
}
