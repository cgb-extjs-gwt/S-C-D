using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Core.Comparators;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
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
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InSingletonScope();
            Bind<IRepositorySet, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InSingletonScope();
            Bind<FrieseEntities>().ToSelf().InSingletonScope();
            Bind<ISqlRepository>().To<SqlRepository>().InSingletonScope();
            Bind<ILogger<LogLevel>>().To<Core.Impl.Logger>().InSingletonScope();

            Bind(typeof(IDataImporter<>)).To(typeof(PorDataImporter<>)).InSingletonScope();

            Bind<IPorSFabsService>().To<PorSFabService>();
            Bind<IPorSogService>().To<PorSogService>();
            Bind<IPorWgService>().To<PorWgService>();
            Bind<IPorSwDigitService>().To<PorSwDigitService>();
            Bind<IPorSwLicenseService>().To<PorSwLicenseService>();
            Bind<IPorSwDigitLicenseService>().To<PorSwDigitLicenseService>();
            Bind<IHwFspCodeTranslationService>().To<PorHwFspCodeTranslationService>();
            Bind<ISwFspCodeTranslationService>().To<PorSwFspCodeTranslationService>();

            //Comparators
            Bind(typeof(IEqualityComparer<>)).To(typeof(PorEqualityComparer<>));


            //Domain Services
            Bind(typeof(ImportService<>)).ToSelf();
            Bind(typeof(DomainService<>)).ToSelf();


            Kernel.RegisterEntity<Pla>();
            Kernel.RegisterEntity<Sog>();
            Kernel.RegisterEntity<SFab>();
            Kernel.RegisterEntity<Wg>();
            Kernel.RegisterEntity<SwDigit>();
            Kernel.RegisterEntity<SwDigitLicense>();
            Kernel.RegisterEntity<SwLicense>();
            Kernel.RegisterEntity<Availability>();
            Kernel.RegisterEntity<ReactionTime>();
            Kernel.RegisterEntity<ReactionType>();
            Kernel.RegisterEntity<ServiceLocation>();
            Kernel.RegisterEntity<Duration>();
            Kernel.RegisterEntity<ProActiveSla>();
            Kernel.RegisterEntity<CountryGroup>();
            Kernel.RegisterEntity<Country>();
            Kernel.RegisterEntity<HwFspCodeTranslation>();
            Kernel.RegisterEntity<SwFspCodeTranslation>();
        }
    }
}
