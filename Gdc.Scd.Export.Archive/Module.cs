using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive.Impl;
using Gdc.Scd.Import.Core.Impl;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.Export.Archive
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepositorySet, IRegisteredEntitiesProvider, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InTransientScope();
            Bind<ILogger>().To<Logger>().InTransientScope();
            Bind<IArchiveRepository>().To<ArchiveRepository>().InTransientScope();
            //Bind<ArchiveService>().ToSelf().InTransientScope();
        }

        public static StandardKernel CreateKernel()
        {
            NinjectExt.IsConsoleApplication = true;
            return new StandardKernel(
                new Module()
            );
        }
    }
}
