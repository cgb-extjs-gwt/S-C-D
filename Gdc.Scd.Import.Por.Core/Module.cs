using Gdc.Scd.Core.Comparators;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Impl;
using Gdc.Scd.Import.Por.Core.Interfaces;
using Ninject.Activation;
using Ninject.Modules;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IRepository<>)).To(typeof(PorModifiableDecoratorRepository<>))
                                       .When(this.IsModifiable)
                                       .InSingletonScope();

            Bind(typeof(PorModifiableDecoratorRepository<>)).ToSelf().InSingletonScope();
            Bind<FrieseEntities>().ToSelf().InSingletonScope();
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
            Bind<ICostBlockUpdateService>().To<CostBlockUpdateService>();
            //Comparators
            Bind(typeof(IEqualityComparer<>)).To(typeof(PorEqualityComparer<>));

            //Domain Services
            Bind(typeof(ImportService<>)).ToSelf();
        }

        private bool IsModifiable(IRequest request)
        {
            var entityType = request.Service.GetGenericArguments().First();

            return typeof(IModifiable).IsAssignableFrom(entityType);
        }
    }
}
