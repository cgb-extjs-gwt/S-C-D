 using Gdc.Scd.Core.Interfaces;
 using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.Export.Sap.Enitities;
using Gdc.Scd.Export.Sap.Impl;
using Gdc.Scd.Export.Sap.Interfaces;
using Gdc.Scd.Spooler.Core;

namespace Gdc.Scd.Export.Sap
{
    public class Module : BaseJobModule<Module>
    {
        public override void Load()
        {
            this.Bind<ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();

            this.Bind<ISapExportLogService>().To<SapExportLogService>().InSingletonScope();
            this.Bind<IManualCostExportService>().To<ManualCostExportService>().InSingletonScope();

            this.Kernel.RegisterEntity<SapExportLog>();
            this.Kernel.RegisterEntity<SapTable>();
        }
    }
}
