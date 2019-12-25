using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;

namespace Gdc.Scd.Import.Ebis.MCiW
{
    public class MaterialCostService
    {
        private IConfigHandler ConfigHandler;
        
        private IImportManager ImportManager;
        
        private ILogger Logger;

        public MaterialCostService(
                IConfigHandler cfg,
                IImportManager import,
                ILogger log
            )
        {
            this.ConfigHandler = cfg;
            this.ImportManager = import;
            this.Logger = log;
        }

        public virtual void Run()
        {
            Logger.Info( ImportConstants.START_PROCESS);
            
            Logger.Info( ImportConstants.CONFIG_READ_START);
            var configuration = ConfigHandler.ReadConfiguration(ImportSystems.EBIS_MATERIAL_COST);
            Logger.Info( ImportConstants.CONFIG_READ_END);
            
            var result = ImportManager.ImportData(configuration);
            if (!result.Skipped)
            {
                Logger.Info( ImportConstants.UPDATING_CONFIGURATION);
                ConfigHandler.UpdateImportResult(configuration, result.ModifiedDateTime);
            }
            
            Logger.Info( ImportConstants.END_PROCESS);
        }
    }
}
