using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Core.Comparators;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Impl;
using Gdc.Scd.Import.Por.Core.Interfaces;
using Ninject.Modules;
using NLog;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<SwDigit>>().To<SwDigitRepository>().InSingletonScope();
            Bind<FrieseEntities>().ToSelf().InSingletonScope();
            Bind<ILogger<LogLevel>, Gdc.Scd.Core.Interfaces.ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();

            Bind(typeof(IDataImporter<>)).To(typeof(PorDataImporter<>)).InSingletonScope();

            Bind<IPorSogService>().To<PorSogService>();
            Bind<IPorWgService>().To<PorWgService>();
            Bind<IPorSwDigitService>().To<PorSwDigitService>();
            Bind<IPorSwSpMaintenaceService>().To<PorSwSpMaintenanceService>();
            Bind<IPorSwLicenseService>().To<PorSwLicenseService>();
            Bind<IPorSwDigitLicenseService>().To<PorSwDigitLicenseService>();
            Bind<IHwFspCodeTranslationService<HwFspCodeDto>>().To<PorHwFspCodeTranslationService>();
            Bind<IHwFspCodeTranslationService<HwHddFspCodeDto>>().To<PorHddHwFspCodeCodeTranslationService>();
            Bind<ISwFspCodeTranslationService>().To<PorSwFspCodeTranslationService>();
            Bind<IPorSwProActiveService>().To<PorSwProActiveService>();
            //Comparators
            Bind(typeof(IEqualityComparer<>)).To(typeof(PorEqualityComparer<>));

            //Domain Services
            Bind(typeof(ImportService<>)).ToSelf();
            Bind(typeof(DomainService<>)).ToSelf();

            this.Bind<IPrincipalProvider>().To<ConsolePrincipleProvider>().InSingletonScope();
        }
    }
}
