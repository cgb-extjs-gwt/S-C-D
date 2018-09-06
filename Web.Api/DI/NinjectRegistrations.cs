using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject.Web.Common;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Interfaces;
using Ninject;
using Gdc.Scd.DataAccessLayer.TestData.Impl;
using Ninject.Web.WebApi.Filter;
using System.Web.Http.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api.DI
{
    public class NinjectRegistrations : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IDomainService<>)).To(typeof(DomainService<>)).InRequestScope();
            Bind<ICostEditorService>().To<CostEditorService>().InRequestScope();
            Bind<ICapabilityMatrixService>().To<CapabilityMatrixService>().InRequestScope();
            Bind<IUserService>().To<UserService>().InRequestScope();
            Bind<ICostBlockHistoryService>().To<CostBlockHistoryService>().InRequestScope();
            Bind<IAvailabilityFeeAdminService>().To<AvailabilityFeeAdminService>().InRequestScope();
            Bind<IEmailService>().To<EmailService>().InRequestScope();
            Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InRequestScope();
            Bind<IDomainMetaSevice>().To<DomainMetaSevice>().InSingletonScope();
            Bind<IDomainEnitiesMetaService>().To<DomainEnitiesMetaService>().InSingletonScope();

            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InRequestScope();
            Bind<EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            Bind<IRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            Bind<ICostEditorRepository>().To<CostEditorRepository>().InRequestScope();
            Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InRequestScope();
            Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            Bind<IRepository<CostBlockHistory>>().To<CostBlockHistoryRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeType>>().To<ReactionTimeTypeRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeAvalability>>().To<ReactionTimeAvalabilityRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeTypeAvalability>>().To<ReactionTimeTypeAvalabilityRepository>().InRequestScope();

            Bind<BaseColumnMetaSqlBuilder<IdFieldMeta>>().To<IdColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<SimpleFieldMeta>>().To<SimpleColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>>().To<ReferenceColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>>().To<CreatedDateTimeColumnMetaSqlBuilder>().InTransientScope();
            Bind<CreateTableMetaSqlBuilder>().To<CreateTableMetaSqlBuilder>().InTransientScope();
            Bind<DatabaseMetaSqlBuilder>().To<DatabaseMetaSqlBuilder>().InTransientScope();
            Bind<IConfigureApplicationHandler>().To<DatabaseCreationHandler>().InTransientScope();
            Bind<IConfigureDatabaseHandler>().To<ViewConfigureHandler>().InTransientScope();
            Bind<ICustomConfigureTableHandler>().To<ViewConfigureHandler>().InTransientScope();
#if DEBUG
            Bind<IConfigureDatabaseHandler>().To<TestDataCreationHandlercs>();
#endif
            Bind<DomainMeta>().ToMethod(context => Kernel.Get<IDomainMetaSevice>().Get()).InSingletonScope();
            Bind<DomainEnitiesMeta>().ToMethod(context =>
            {
                var domainMeta = Kernel.Get<DomainMeta>();
                var domainEntitiesMetaService = Kernel.Get<IDomainEnitiesMetaService>();
                return domainEntitiesMetaService.Get(domainMeta);
            });
        }
    }
}