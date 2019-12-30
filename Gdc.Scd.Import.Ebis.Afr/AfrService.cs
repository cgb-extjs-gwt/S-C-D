using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;

namespace Gdc.Scd.Import.Ebis.Afr
{
    public class AfrService
    {
        private IConfigHandler ConfigHandler;

        private IImportManager ImportManager;

        private ILogger Logger;

        public AfrService(
                IConfigHandler ConfigHandler,
                IImportManager ImportManager,
                ILogger Logger
            )
        {
            this.ConfigHandler = ConfigHandler;
            this.ImportManager = ImportManager;
            this.Logger = Logger;
        }

        public virtual void Run()
        {
            Logger.Info(ImportConstants.START_PROCESS);
            Logger.Info(ImportConstants.CONFIG_READ_START);

            var configuration = ConfigHandler.ReadConfiguration(ImportSystems.EBIS_AFR);
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
