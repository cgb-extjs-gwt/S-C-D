using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using NLog;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Import.CentralContractGroup
{
    public class CentralContractGroupService
    {
        public static IConfigHandler ConfigHandler { get; private set; }
        public static IImportManager ImportManager { get; set; }
        public static ILogger<LogLevel> Logger { get; private set; }

        static CentralContractGroupService()
        {
            IKernel kernel = new StandardKernel(new Module());
            ConfigHandler = kernel.Get<IConfigHandler>();
            ImportManager = kernel.Get<IImportManager>();
            Logger = kernel.Get<ILogger<LogLevel>>();
        }

        public static void UploadCentralContractGroups()
        {
            Logger.Log(LogLevel.Info, ImportConstants.START_PROCESS);
            Logger.Log(LogLevel.Info, ImportConstants.CONFIG_READ_START);
            var configuration = ConfigHandler.ReadConfiguration("Default");
            Logger.Log(LogLevel.Info, ImportConstants.CONFIG_READ_END);
            var skipped = ImportManager.ImportData(configuration);
            Logger.Log(LogLevel.Info, ImportConstants.END_PROCESS);
        }
    }
}
