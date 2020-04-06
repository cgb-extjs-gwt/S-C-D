using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.ArchiveJob;
using Gdc.Scd.Export.ArchiveResultSenderJob;
using Gdc.Scd.Export.CdCsJob;
using Gdc.Scd.Export.Sap.Impl;
using Gdc.Scd.Import.AmberRoad;
using Gdc.Scd.Import.CentralContractGroup;
using Gdc.Scd.Import.Ebis.Afr;
using Gdc.Scd.Import.Ebis.InstallBase;
using Gdc.Scd.Import.Ebis.MCiW;
using Gdc.Scd.Import.ExchangeRatesJob;
using Gdc.Scd.Import.Logistics;
using Gdc.Scd.Import.Por;
using Gdc.Scd.Import.SfabImport;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject.Modules;

namespace Gdc.Scd.Spooler
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IJob>().To<ArchiveJob>().InSingletonScope();
            Bind<IJob>().To<ArchiveResultSenderJob>().InSingletonScope();
            Bind<IJob>().To<CdCsJob>().InSingletonScope();
            Bind<IJob>().To<AmberRoadJob>().InSingletonScope();
            Bind<IJob>().To<CentralContractGroupJob>().InSingletonScope();
            Bind<IJob>().To<AfrJob>().InSingletonScope();
            Bind<IJob>().To<InstallBaseJob>().InSingletonScope();
            Bind<IJob>().To<MCiWJob>().InSingletonScope();
            Bind<IJob>().To<ExchangeRatesJob>().InSingletonScope();
            Bind<IJob>().To<LogisticsJob>().InSingletonScope();
            Bind<IJob>().To<ImportPorJob>().InSingletonScope();
            Bind<IJob>().To<SfabJob>().InSingletonScope();
            Bind<IJob>().To<SapJob>().InSingletonScope();
        }
    }
}
