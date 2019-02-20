using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Ninject;
using Ninject.Modules;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.CentralContractGroup
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IRepository<Wg>)).To(typeof(ImportRepository<Wg>)).InSingletonScope();
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InSingletonScope();
            Bind<IRepositorySet, EntityFrameworkRepositorySet, IRegisteredEntitiesProvider>().To<EntityFrameworkRepositorySet>().InSingletonScope();

            Bind<ISqlRepository>().To<SqlRepository>().InSingletonScope();
            Bind<ILogger<LogLevel>>().To<Core.Impl.Logger>().InSingletonScope();

            Bind<IDownloader>().To<FileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(CentralContractGroupUploader)).InSingletonScope();
            Bind<IImportManager>().To<ImportManager<CentralContractGroupDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<FileConfigHandler>().InSingletonScope();

            //Cost Blocks and Meta 
            Bind<ICostBlockRepository>().To<CostBlockRepository>().InSingletonScope();
            Bind<ICostBlockService>().To<CostBlockService>();
            Bind<ICoordinateEntityMetaProvider>().To<CustomCoordinateMetaProvider>();

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

            Bind<IUserService>().To<UserService>().InSingletonScope();
            this.Bind<Scd.Core.Interfaces.IPrincipalProvider>().To<ConsolePrincipleProvider>().InSingletonScope();
            Bind<IUserRepository, IRepository<User>>().To<UserRepository>().InSingletonScope();
            Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InSingletonScope();
            Bind<IQualityGateRepository>().To<QualityGateRepository>().InSingletonScope();
            Bind<IQualityGateQueryBuilder>().To<QualityGateQueryBuilder>().InSingletonScope();
            Bind<IQualityGateSevice>().To<QualityGateSevice>().InSingletonScope();
            Bind<ICostBlockValueHistoryQueryBuilder>().To<CostBlockValueHistoryQueryBuilder>().InSingletonScope();
            Bind<ICostBlockHistoryService>().To<CostBlockHistoryService>().InSingletonScope();
            Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InSingletonScope();

            Kernel.RegisterEntity<Wg>();
            Kernel.RegisterEntity<Gdc.Scd.Core.Entities.CentralContractGroup>();
  
        }
    }
}
