using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Comparators;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Impl;
using Gdc.Scd.Import.Por.Core.Interfaces;
using Ninject;
using Ninject.Modules;
using NLog;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<SwDigit>>().To<SwDigitRepository>().InSingletonScope();
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InSingletonScope();
            Bind<IRepositorySet, EntityFrameworkRepositorySet, IRegisteredEntitiesProvider>().To<EntityFrameworkRepositorySet>().InSingletonScope();
            Bind<FrieseEntities>().ToSelf().InSingletonScope();
            Bind<ISqlRepository>().To<SqlRepository>().InSingletonScope();
            Bind<ICostBlockRepository>().To<CostBlockRepository>().InSingletonScope();
            Bind<ILogger<LogLevel>>().To<Import.Core.Impl.Logger>().InSingletonScope();

            Bind(typeof(IDataImporter<>)).To(typeof(PorDataImporter<>)).InSingletonScope();

            Bind<IPorSogService>().To<PorSogService>();
            Bind<IPorWgService>().To<PorWgService>();
            Bind<IPorSwDigitService>().To<PorSwDigitService>();
            Bind<IPorSwLicenseService>().To<PorSwLicenseService>();
            Bind<IPorSwDigitLicenseService>().To<PorSwDigitLicenseService>();
            Bind<IHwFspCodeTranslationService<HwFspCodeDto>>().To<PorHwFspCodeTranslationService>();
            Bind<IHwFspCodeTranslationService<HwHddFspCodeDto>>().To<PorHddHwFspCodeCodeTranslationService>();
            Bind<ISwFspCodeTranslationService>().To<PorSwFspCodeTranslationService>();
            Bind<IPorSwProActiveService>().To<PorSwProActiveService>();
            Bind<ICostBlockService>().To<CostBlockService>();
            Bind<ICoordinateEntityMetaProvider>().To<CustomCoordinateMetaProvider>();
            //Comparators
            Bind(typeof(IEqualityComparer<>)).To(typeof(PorEqualityComparer<>));


            //Domain Services
            Bind(typeof(ImportService<>)).ToSelf();
            Bind(typeof(DomainService<>)).ToSelf();
            Bind<IDomainMetaSevice>().To<DomainMetaSevice>().InSingletonScope();
            Bind<IDomainEnitiesMetaService>().To<DomainEnitiesMetaService>().InSingletonScope();
            
            Bind<DomainMeta>().ToMethod(context => Kernel.Get<IDomainMetaSevice>().Get()).InSingletonScope();
            Bind<DomainEnitiesMeta>().ToMethod(context =>
            {
                var domainMeta = Kernel.Get<DomainMeta>();
                var domainEntitiesMetaService = Kernel.Get<IDomainEnitiesMetaService>();
                return domainEntitiesMetaService.Get(domainMeta);
            }).InSingletonScope();

            Kernel.RegisterEntity<Pla>();
            Kernel.RegisterEntity<CentralContractGroup>();
            Kernel.RegisterEntity<Sog>();
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
            Kernel.RegisterEntity<ProActiveDigit>();
        }
    }
}
