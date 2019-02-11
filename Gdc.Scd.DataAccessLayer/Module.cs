using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Ninject.Modules;
using Ninject.Web.Common;

namespace Gdc.Scd.DataAccessLayer
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InRequestScope();
            Bind<IRepositorySet, IRegisteredEntitiesProvider, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            Bind<ICostEditorRepository>().To<CostEditorRepository>().InRequestScope();
            Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InRequestScope();
            Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            Bind<IRepository<CostBlockHistory>>().To<CostBlockHistoryRepository>().InRequestScope();
            Bind<IWgRepository, IRepository<Wg>>().To<WgRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeType>>().To<ReactionTimeTypeRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeAvalability>>().To<ReactionTimeAvalabilityRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeTypeAvalability>>().To<ReactionTimeTypeAvalabilityRepository>().InRequestScope();
            Bind<ICostBlockValueHistoryQueryBuilder>().To<CostBlockValueHistoryQueryBuilder>().InRequestScope();
            Bind<IRepository<DurationAvailability>>().To<DurationAvailabilityRepository>().InRequestScope();
            Bind<IQualityGateRepository>().To<QualityGateRepository>().InRequestScope();
            Bind<IQualityGateQueryBuilder>().To<QualityGateQueryBuilder>().InRequestScope();
            Bind<IRepository<Country>>().To<CountryRepository>().InRequestScope();
            Bind<ITableViewRepository>().To<TableViewRepository>().InRequestScope();
            Bind<IRepository<Role>>().To<RoleRepository>().InRequestScope();
            Bind<IUserRepository, IRepository<User>>().To<UserRepository>().InRequestScope();
            Bind<ICostBlockRepository>().To<CostBlockRepository>().InRequestScope();
            Bind<IApprovalRepository>().To<ApprovalRepository>().InRequestScope();
            Bind<IRepository<HardwareManualCost>>().To<HardwareManualCostRepository>().InRequestScope();
            Bind<IRepository<HddRetentionManualCost>>().To<HddRetentionManualCostRepository>().InRequestScope();

            Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InRequestScope();

            Bind<BaseColumnMetaSqlBuilder<IdFieldMeta>>().To<IdColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<SimpleFieldMeta>>().To<SimpleColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>>().To<ReferenceColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>>().To<CreatedDateTimeColumnMetaSqlBuilder>().InTransientScope();
            Bind<CreateTableMetaSqlBuilder>().To<CreateTableMetaSqlBuilder>().InTransientScope();
            Bind<DatabaseMetaSqlBuilder>().To<DatabaseMetaSqlBuilder>().InTransientScope();
            Bind<IConfigureApplicationHandler>().To<DatabaseCreationHandler>().InTransientScope();
            Bind<IConfigureDatabaseHandler, ICustomConfigureTableHandler, ICoordinateEntityMetaProvider>().To<ViewConfigureHandler>().InTransientScope();
            Bind<IConfigureDatabaseHandler>().To<CountryViewConfigureHandler>().InTransientScope();

            Kernel.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(CostElementContext), nameof(CostBlockHistory.Context)));
            Kernel.RegisterEntityAsUnique<User>(nameof(User.Login));
            Kernel.RegisterEntity<UserRole>();
            Kernel.RegisterEntityAsUniqueName<Role>();
            Kernel.RegisterEntityAsUniqueName<Permission>();
            Kernel.RegisterEntity<RolePermission>();
        }
    }
}
