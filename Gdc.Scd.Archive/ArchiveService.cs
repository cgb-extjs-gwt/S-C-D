using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Archive
{
    public class ArchiveService
    {
        private ILogger logger;

        public ArchiveService(ILogger logger)
        {
            this.logger = logger;
        }

        public virtual void Run()
        {
            logger.Info(ArchiveConstants.START_PROCESS);
            //
            logger.Info(ArchiveConstants.END_PROCESS);
        }
    }
}
