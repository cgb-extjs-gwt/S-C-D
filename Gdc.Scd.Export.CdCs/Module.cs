using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.CdCs.Procedures;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.Export.CdCs
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InSingletonScope();
            Bind<IRepositorySet, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InSingletonScope();
            Bind<ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();


            Bind(typeof(GetServiceCostsBySla)).ToSelf();
            Bind(typeof(GetProActiveCosts)).ToSelf();
            Bind(typeof(GetHddRetentionCosts)).ToSelf();
            Bind(typeof(ConfigHandler)).ToSelf();
            Bind(typeof(SpFileDownloader)).ToSelf();

            Kernel.RegisterEntity<Country>();
            Kernel.RegisterEntity<CdCsConfiguration>();
        }

        public static StandardKernel CreateKernel()
        {
            return new StandardKernel(new Module());
        }
    }
}
