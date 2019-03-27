using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using NLog;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;

namespace Gdc.Scd.Import.CentralContractGroup
{
    public class CentralContractGroupService
    {
        public IConfigHandler ConfigHandler { get; private set; }
        public IImportManager ImportManager { get; set; }
        public ILogger<LogLevel> Logger { get; private set; }
        public ICostBlockService CostBlockService { get; private set; }

        public CentralContractGroupService()
        {
            NinjectExt.IsConsoleApplication = true;
            IKernel kernel = CreateKernel();
            ImportManager = kernel.Get<IImportManager>();
            Logger = kernel.Get<ILogger<LogLevel>>();
            CostBlockService = kernel.Get<ICostBlockService>();
        }

        public void UploadCentralContractGroups()
        {
            Logger.Log(LogLevel.Info, ImportConstants.START_PROCESS); 
            var result = ImportManager.ImportData(null);
            if (!result.Skipped)
                UpdateCostBlocks(result.UpdateOptions);
            Logger.Log(LogLevel.Info, ImportConstants.END_PROCESS);
        }

        public void UpdateCostBlocks(IEnumerable<UpdateQueryOption> updateOptions)
        {
            Logger.Log(LogLevel.Info, ImportConstants.UPDATE_COST_BLOCKS_START);
            CostBlockService.UpdateByCoordinates(updateOptions);
            Logger.Log(LogLevel.Info, ImportConstants.UPDATE_COST_BLOCKS_END);
        }

        private static StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Scd.Core.Module(),
                new Scd.DataAccessLayer.Module(),
                new Scd.BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
