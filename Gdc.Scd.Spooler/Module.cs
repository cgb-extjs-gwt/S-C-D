using System.Linq;
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
            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(ArchiveJob.JobName))
                Bind<IJob>().To<ArchiveJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(ArchiveResultSenderJob.JobName))
                Bind<IJob>().To<ArchiveResultSenderJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(CdCsJob.JobName))
                Bind<IJob>().To<CdCsJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(AmberRoadJob.JobName))
                Bind<IJob>().To<AmberRoadJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(CentralContractGroupJob.JobName))
                Bind<IJob>().To<CentralContractGroupJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(AfrJob.JobName))
                Bind<IJob>().To<AfrJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(InstallBaseJob.JobName))
                Bind<IJob>().To<InstallBaseJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(MCiWJob.JobName))
                Bind<IJob>().To<MCiWJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(ExchangeRatesJob.JobName))
                Bind<IJob>().To<ExchangeRatesJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(LogisticsJob.JobName))
                Bind<IJob>().To<LogisticsJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(ImportPorJob.JobName))
                Bind<IJob>().To<ImportPorJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(SfabJob.JobName))
                Bind<IJob>().To<SfabJob>().InSingletonScope();

            if (!Config.RunOnlyJobs.Any() || Config.RunOnlyJobs.Contains(SapJob.JobName))
                Bind<IJob>().To<SapJob>().InSingletonScope();
        }
    }
}
