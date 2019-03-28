using Gdc.Scd.OperationResult;
using System;

namespace Gdc.Scd.Archive
{
    public class ArchiveJob
    {
        public const string ARCHIVE_UNEXPECTED_ERROR = "Archivation completed unsuccessfully. Please find details below.";

        protected ArchiveService srv;

        public ArchiveJob()
        {
            srv = new ArchiveService();
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
                Notify(ARCHIVE_UNEXPECTED_ERROR, e);
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
            return "ArchiveJob";
        }

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
