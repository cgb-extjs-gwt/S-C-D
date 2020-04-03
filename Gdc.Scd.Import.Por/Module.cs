using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Ninject;
using Ninject.Modules;
using NLog;

namespace Gdc.Scd.Import.Por
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<SwDigit>>().To<SwDigitRepository>().InSingletonScope();
            Bind<ILogger<LogLevel>, Gdc.Scd.Core.Interfaces.ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();
            
            Bind(typeof(DomainService<>)).ToSelf();

            this.Bind<IPrincipalProvider>().To<ConsolePrincipleProvider>().InSingletonScope();
        }

        public static StandardKernel CreateKernel()
        {
            NinjectExt.IsConsoleApplication = true;
            return new StandardKernel(
                new Import.Por.Core.Module(),
                new Module(),
                new BusinessLogicLayer.Module(),
                new DataAccessLayer.Module { ExcludeModifiableDecoratorRepository = true },
                new Scd.Core.Module());
        }
    }
}
