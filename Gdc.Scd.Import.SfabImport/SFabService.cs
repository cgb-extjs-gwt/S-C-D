using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.SfabImport
{
    public static class SFabService
    {
        public static IConfigHandler ConfigHandler { get; private set; }
        public static IImportManager ImportManager { get; set; }
        public static ILogger<LogLevel> Logger { get; private set; }

        static SFabService()
        {
            IKernel kernel = new StandardKernel(new Module());
            ConfigHandler = kernel.Get<IConfigHandler>();
            ImportManager = kernel.Get<IImportManager>();
            Logger = kernel.Get<ILogger<LogLevel>>();
        }

        public static void UploadSfabs()
        {
            Logger.Log(LogLevel.Info, ImportConstants.START_PROCESS);
            Logger.Log(LogLevel.Info, ImportConstants.CONFIG_READ_START);
            var configuration = ConfigHandler.ReadConfiguration(ImportSystems.SFABS);
            Logger.Log(LogLevel.Info, ImportConstants.CONFIG_READ_END);
            var skipped = ImportManager.ImportData(configuration);
            if (!skipped)
            {
                Logger.Log(LogLevel.Info, ImportConstants.UPDATING_CONFIGURATION);
                ConfigHandler.UpdateImportResult(configuration, DateTime.Now);
            }
            Logger.Log(LogLevel.Info, ImportConstants.END_PROCESS);
        }
    }
}
