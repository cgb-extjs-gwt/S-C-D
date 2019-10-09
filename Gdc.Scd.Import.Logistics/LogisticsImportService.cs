using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;

namespace Gdc.Scd.Import.Logistics
{
    public class LogisticsImportService
    {
        private readonly IConfigHandler configHandler;

        private readonly IImportManager importManager;

        private readonly ILogger logger;

        public LogisticsImportService(
                IConfigHandler config,
                IImportManager manager,
                ILogger log
            )
        {
            configHandler = config;
            importManager = manager;
            logger = log;
        }

        public virtual void Run()
        {
            logger.Info(ImportConstants.START_PROCESS);
            logger.Info(ImportConstants.CONFIG_READ_START);
            var configuration = configHandler.ReadConfiguration(ImportSystems.LOGISTICS);
            logger.Info(ImportConstants.CONFIG_READ_END);
            var result = importManager.ImportData(configuration);
            if (!result.Skipped)
            {
                logger.Info(ImportConstants.UPDATING_CONFIGURATION);
                configHandler.UpdateImportResult(configuration, result.ModifiedDateTime);
            }
            logger.Info(ImportConstants.END_PROCESS);
        }
    }
}
