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

namespace Gdc.Scd.Import.Logistics
{
    public static class LogisticsImportService
    {
        public static IConfigHandler ConfigHandler { get; private set; }
        public static IImportManager ImportManager { get; set; }
        public static ILogger<LogLevel> Logger { get; private set; }

        static LogisticsImportService()
        {
            IKernel kernel = new StandardKernel(new Module());
            ConfigHandler = kernel.Get<IConfigHandler>();
            ImportManager = kernel.Get<IImportManager>();
            Logger = kernel.Get<ILogger<LogLevel>>();
        }

        public static void UploadLogisticInfo()
        {
            Logger.Log(LogLevel.Info, ImportConstants.START_PROCESS);
            Logger.Log(LogLevel.Info, ImportConstants.CONFIG_READ_START);
            var configuration = ConfigHandler.ReadConfiguration(ImportSystems.LOGISTICS);
            Logger.Log(LogLevel.Info, ImportConstants.CONFIG_READ_END);
            ImportManager.ImportData(configuration);
            Logger.Log(LogLevel.Info, ImportConstants.UPDATING_CONFIGURATION);
            ConfigHandler.UpdateImportResult(configuration, DateTime.Now);
            Logger.Log(LogLevel.Info, ImportConstants.END_PROCESS);
        }
    }
}
