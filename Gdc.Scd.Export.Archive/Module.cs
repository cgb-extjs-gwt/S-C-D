using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Impl;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.Export.Archive
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ILogger>().To<Logger>().InSingletonScope();
            Bind<ArchiveService>().ToSelf().InSingletonScope();
        }

        public static StandardKernel CreateKernel()
        {
            NinjectExt.IsConsoleApplication = true;
            return new StandardKernel(
                new Scd.Core.Module(),
                new Scd.DataAccessLayer.Module(),
                new Module());
        }
    }
}
