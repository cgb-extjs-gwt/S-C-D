using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Ninject.Activation;
using Ninject.Modules;
using System;

namespace Gdc.Scd.DataAccessLayer
{
    public class Module : NinjectModule
    {
        public bool ExcludeModifiableDecoratorRepository { get; set; }

        public override void Load()
        {
            if (!this.ExcludeModifiableDecoratorRepository)
            {
                Bind(typeof(IRepository<>)).To(typeof(ModifiableDecoratorRepository<>)).When(this.IsModifiable).InScdRequestScope();
            }
            
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InScdRequestScope();
            Bind<IRepositorySet, IRegisteredEntitiesProvider, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InScdRequestScope();
            Bind<ICostEditorRepository>().To<CostEditorRepository>().InScdRequestScope();
            Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InScdRequestScope();
            Bind<ISqlRepository>().To<SqlRepository>().InScdRequestScope();
            Bind<IRepository<CostBlockHistory>>().To<CostBlockHistoryRepository>().InScdRequestScope();
            Bind<IWgRepository, IRepository<Wg>>().To<WgRepository>().InScdRequestScope();
            Bind<IRepository<ReactionTimeType>>().To<ReactionTimeTypeRepository>().InScdRequestScope();
            Bind<IRepository<ReactionTimeAvalability>>().To<ReactionTimeAvalabilityRepository>().InScdRequestScope();
            Bind<IRepository<ReactionTimeTypeAvalability>>().To<ReactionTimeTypeAvalabilityRepository>().InScdRequestScope();
            Bind<ICostBlockValueHistoryQueryBuilder>().To<CostBlockValueHistoryQueryBuilder>().InScdRequestScope();
            Bind<IRepository<DurationAvailability>>().To<DurationAvailabilityRepository>().InScdRequestScope();
            Bind<IQualityGateRepository>().To<QualityGateRepository>().InScdRequestScope();
            Bind<IQualityGateQueryBuilder>().To<QualityGateQueryBuilder>().InScdRequestScope();
            Bind<IRepository<Country>>().To<CountryRepository>().InScdRequestScope();
            Bind<ITableViewRepository>().To<TableViewRepository>().InScdRequestScope();
            Bind<IRepository<Role>>().To<RoleRepository>().InScdRequestScope();
            Bind<IUserRepository, IRepository<User>>().To<UserRepository>().InScdRequestScope();
            Bind<ICostBlockRepository>().To<CostBlockRepository>().InScdRequestScope();
            Bind<IApprovalRepository>().To<ApprovalRepository>().InScdRequestScope();
            Bind<IRepository<HardwareManualCost>>().To<HardwareManualCostRepository>().InScdRequestScope();
            Bind<IRepository<HddRetentionManualCost>>().To<HddRetentionManualCostRepository>().InScdRequestScope();
            Bind<IRepository<StandardWarrantyManualCost>>().To<StandardWarrantyManualCostRepository>().InScdRequestScope();
            Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InScdRequestScope();
            Bind<ICostBlockQueryBuilder>().To<CostBlockQueryBuilder>().InScdRequestScope();
            Bind<IPivotGridRepository>().To<PivotGridRepository>().InScdRequestScope();
            Bind<IPortfolioPivotGridQueryBuilder>().To<PortfolioPivotGridQueryBuilder>().InSingletonScope();
            Bind<IPortfolioRepository<PrincipalPortfolio, PrincipalPortfolioInheritance>, IRepository<PrincipalPortfolio>>().To<PortfolioRepository<PrincipalPortfolio, PrincipalPortfolioInheritance>>().InScdRequestScope();
            Bind<IPortfolioRepository<LocalPortfolio, LocalPortfolioInheritance>, IRepository<LocalPortfolio>>().To<PortfolioRepository<LocalPortfolio, LocalPortfolioInheritance>>().InScdRequestScope();

            Bind<BaseColumnMetaSqlBuilder<IdFieldMeta>>().To<IdColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<SimpleFieldMeta>>().To<SimpleColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>>().To<ReferenceColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>>().To<CreatedDateTimeColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<ComputedFieldMeta>>().To<ComputedColumnMetaSqlBuilder>().InTransientScope();
            Bind<CreateTableMetaSqlBuilder>().To<CreateTableMetaSqlBuilder>().InTransientScope();
            Bind<AlterTableMetaSqlBuilder>().To<AlterTableMetaSqlBuilder>().InTransientScope();
            Bind<DatabaseMetaSqlBuilder>().To<DatabaseMetaSqlBuilder>().InTransientScope();
            Bind<IConfigureApplicationHandler>().To<DatabaseCreationHandler>().InTransientScope();
            Bind<IConfigureDatabaseHandler, ICustomConfigureTableHandler, ICoordinateEntityMetaProvider>().To<ViewConfigureHandler>().InTransientScope();
            Bind<IConfigureDatabaseHandler>().To<CountryViewConfigureHandler>().InTransientScope();
            Bind<IFieldBuilder>().To<FieldBuilder>().InTransientScope();

            Kernel.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(CostElementContext), nameof(CostBlockHistory.Context)));
            Kernel.RegisterEntityAsUnique<User>(nameof(User.Login));
            Kernel.RegisterEntity<UserRole>();
            Kernel.RegisterEntityAsUniqueName<Role>();
            Kernel.RegisterEntityAsUniqueName<Permission>();
            Kernel.RegisterEntity<RolePermission>();
        }

        private bool IsModifiable(IRequest arg)
        {
            var type = arg.Service.GetGenericArguments();
            var deactivatable = typeof(IModifiable);
            return Array.Exists(type, x => deactivatable.IsAssignableFrom(x));
        }
    }
}
