using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.TestData.Impl;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Ninject.Web.Common;

namespace Web.Api.DI
{
    public class ScdNinjectDependencyResolver : System.Web.Mvc.IDependencyResolver,
        IDependencyScope,
        System.Web.Http.Dependencies.IDependencyResolver
    {
        private IKernel kernel;

        public ScdNinjectDependencyResolver(IKernel kernel)
        {
            this.kernel = kernel;
            AddBindings();
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
            
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            kernel.Bind(typeof(IDomainService<>)).To(typeof(DomainService<>)).InRequestScope();
            kernel.Bind<ICostEditorService>().To<CostEditorService>().InRequestScope();
            kernel.Bind<ICapabilityMatrixService>().To<CapabilityMatrixService>().InRequestScope();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope();
            kernel.Bind<ICostBlockHistoryService>().To<CostBlockHistoryService>().InRequestScope();
            kernel.Bind<IAvailabilityFeeAdminService>().To<AvailabilityFeeAdminService>().InRequestScope();
            kernel.Bind<IEmailService>().To<EmailService>().InRequestScope();
            kernel.Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InRequestScope();
            kernel.Bind<IDomainMetaSevice>().To<DomainMetaSevice>().InSingletonScope();
            kernel.Bind<IDomainEnitiesMetaService>().To<DomainEnitiesMetaService>().InSingletonScope();

            kernel.Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InRequestScope();
            kernel.Bind<EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            kernel.Bind<IRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            kernel.Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            kernel.Bind<ICostEditorRepository>().To<CostEditorRepository>().InRequestScope();
            kernel.Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InRequestScope();
            kernel.Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            kernel.Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            kernel.Bind<IRepository<CostBlockHistory>>().To<CostBlockHistoryRepository>().InRequestScope();
            kernel.Bind<IRepository<ReactionTimeType>>().To<ReactionTimeTypeRepository>().InRequestScope();
            kernel.Bind<IRepository<ReactionTimeAvalability>>().To<ReactionTimeAvalabilityRepository>().InRequestScope();
            kernel.Bind<IRepository<ReactionTimeTypeAvalability>>().To<ReactionTimeTypeAvalabilityRepository>().InRequestScope();

            kernel.Bind<BaseColumnMetaSqlBuilder<IdFieldMeta>>().To<IdColumnMetaSqlBuilder>().InTransientScope();
            kernel.Bind<BaseColumnMetaSqlBuilder<SimpleFieldMeta>>().To<SimpleColumnMetaSqlBuilder>().InTransientScope();
            kernel.Bind<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>>().To<ReferenceColumnMetaSqlBuilder>().InTransientScope();
            kernel.Bind<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>>().To<CreatedDateTimeColumnMetaSqlBuilder>().InTransientScope();
            kernel.Bind<CreateTableMetaSqlBuilder>().To<CreateTableMetaSqlBuilder>().InTransientScope();
            kernel.Bind<DatabaseMetaSqlBuilder>().To<DatabaseMetaSqlBuilder>().InTransientScope();
            kernel.Bind<IConfigureApplicationHandler>().To<DatabaseCreationHandler>().InTransientScope();
            kernel.Bind<IConfigureDatabaseHandler>().To<ViewConfigureHandler>().InTransientScope();
            kernel.Bind<ICustomConfigureTableHandler>().To<ViewConfigureHandler>().InTransientScope();
#if DEBUG
            kernel.Bind<IConfigureDatabaseHandler>().To<TestDataCreationHandlercs>();
#endif
            kernel.Bind<DomainMeta>().ToMethod(context => kernel.Get<IDomainMetaSevice>().Get()).InSingletonScope();
            kernel.Bind<DomainEnitiesMeta>().ToMethod(context =>
            {
                var domainMeta = kernel.Get<DomainMeta>();
                var domainEntitiesMetaService = kernel.Get<IDomainEnitiesMetaService>();
                return domainEntitiesMetaService.Get(domainMeta);
            });
        }
    }
}