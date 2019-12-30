using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;

namespace Gdc.Scd.Import.AmberRoad
{
    public class AmberRoadService
    {
        private IConfigHandler ConfigHandler;

        private IImportManager ImportManager;

        private ILogger Logger;

        public AmberRoadService(
                IImportManager import,
                IConfigHandler cfg,
                ILogger logger
            )
        {
            this.ImportManager = import;
            this.ConfigHandler = cfg;
            this.Logger = logger;
        }

        public virtual void Run()
        {
            Logger.Info(ImportConstants.START_PROCESS);
            Logger.Info(ImportConstants.CONFIG_READ_START);

            var configuration = ConfigHandler.ReadConfiguration(ImportSystems.AMBERROAD);

            Logger.Info(ImportConstants.CONFIG_READ_END);

            var result = ImportManager.ImportData(configuration);
            if (!result.Skipped)
            {
                Logger.Info(ImportConstants.UPDATING_CONFIGURATION);
                ConfigHandler.UpdateImportResult(configuration, result.ModifiedDateTime);
            }

            Logger.Info(ImportConstants.END_PROCESS);
        }
    }
}
