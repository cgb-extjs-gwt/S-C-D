using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Core.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.Import.CentralContractGroup
{
    public class CentralContractGroupService
    {
        private IImportManager ImportManager;

        private ICostBlockService CostBlockService;

        private ILogger Logger;

        public CentralContractGroupService(
                IImportManager import,
                ICostBlockService costbock,
                ILogger log
            )
        {
            this.ImportManager = import;
            this.CostBlockService = costbock;
            this.Logger = log;
        }

        public virtual void UploadCentralContractGroups()
        {
            Logger.Info(ImportConstants.START_PROCESS);
            
            var result = ImportManager.ImportData(null);
            if (!result.Skipped)
            {
                UpdateCostBlocks(result.UpdateOptions);
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
