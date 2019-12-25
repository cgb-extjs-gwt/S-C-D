using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Core.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.Import.SfabImport
{
    public class SFabService
    {
        private IConfigHandler ConfigHandler;

        private IImportManager ImportManager;

        private ICostBlockService CostBlockService;

        private ILogger Logger;

        public SFabService(
                IConfigHandler cfg,
                IImportManager import,
                ICostBlockService costblock,
                ILogger log
            )
        {
            this.ConfigHandler = cfg;
            this.ImportManager = import;
            this.CostBlockService = costblock;
            this.Logger = log;
        }

        public virtual void Run()
        {
            Logger.Info(ImportConstants.START_PROCESS);

            Logger.Info(ImportConstants.CONFIG_READ_START);
            var configuration = ConfigHandler.ReadConfiguration(ImportSystems.SFABS);
            Logger.Info(ImportConstants.CONFIG_READ_END);

            var result = ImportManager.ImportData(configuration);

            if (!result.Skipped)
            {
                UpdateCostBlocks(result.UpdateOptions);
                Logger.Info(ImportConstants.UPDATING_CONFIGURATION);
                ConfigHandler.UpdateImportResult(configuration, result.ModifiedDateTime);
            }

            Logger.Info(ImportConstants.END_PROCESS);
        }

        public void UpdateCostBlocks(IEnumerable<UpdateQueryOption> updateOptions)
        {
            Logger.Info(ImportConstants.UPDATE_COST_BLOCKS_START);
            CostBlockService.UpdateByCoordinates(updateOptions);
            Logger.Info(ImportConstants.UPDATE_COST_BLOCKS_END);
        }
    }
}
