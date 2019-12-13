using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Export.ArchiveResultSender.Abstract;
using Gdc.Scd.Export.ArchiveResultSender.Concrete;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.Export.ArchiveResultSender
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<Gdc.Scd.Core.Interfaces.ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();
            Bind<IArchiveInfoGetter>().To<FileSystemArchiveInfoGetter>().InSingletonScope();
            Bind<IEmailService>().To<EmailService>().InSingletonScope();
            Bind<ArchiveResultService>().ToSelf();
        }

        public static StandardKernel CreateKernel()
        {
            return new StandardKernel(new Module());
        }
    }
}
