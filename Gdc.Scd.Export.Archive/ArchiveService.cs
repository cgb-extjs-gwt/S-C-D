using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Export.Archive
{
    public class ArchiveService
    {
        private IArchiveRepository repo;

        private ILogger logger;

        public ArchiveService(IArchiveRepository repo, ILogger logger)
        {
            this.repo = repo;
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
