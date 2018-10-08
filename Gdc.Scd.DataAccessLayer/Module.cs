using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
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
            Bind<IRepositorySet, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            Bind<ICostEditorRepository>().To<CostEditorRepository>().InRequestScope();
            Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InRequestScope();
            Bind<ISqlRepository>().To<SqlRepository>().InRequestScope();
            Bind<IRepository<CostBlockHistory>>().To<CostBlockHistoryRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeType>>().To<ReactionTimeTypeRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeAvalability>>().To<ReactionTimeAvalabilityRepository>().InRequestScope();
            Bind<IRepository<ReactionTimeTypeAvalability>>().To<ReactionTimeTypeAvalabilityRepository>().InRequestScope();
            Bind<ICostBlockValueHistoryQueryBuilder>().To<CostBlockValueHistoryQueryBuilder>().InRequestScope();
            Bind<IRepository<YearAvailability>>().To<YearAvailabilityRepository>().InRequestScope();
            Bind<IQualityGateRepository>().To<QualityGateRepository>().InRequestScope();
            Bind<IQualityGateQueryBuilder>().To<QualityGateQueryBuilder>().InRequestScope();
            Bind<IRepository<Country>>().To<CountryRepository>().InRequestScope();

            Bind<BaseColumnMetaSqlBuilder<IdFieldMeta>>().To<IdColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<SimpleFieldMeta>>().To<SimpleColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>>().To<ReferenceColumnMetaSqlBuilder>().InTransientScope();
            Bind<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>>().To<CreatedDateTimeColumnMetaSqlBuilder>().InTransientScope();
            Bind<CreateTableMetaSqlBuilder>().To<CreateTableMetaSqlBuilder>().InTransientScope();
            Bind<DatabaseMetaSqlBuilder>().To<DatabaseMetaSqlBuilder>().InTransientScope();
            Bind<IConfigureApplicationHandler>().To<DatabaseCreationHandler>().InTransientScope();
            Bind<IConfigureDatabaseHandler>().To<ViewConfigureHandler>().InTransientScope();
            Bind<ICustomConfigureTableHandler>().To<ViewConfigureHandler>().InTransientScope();

            Kernel.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(HistoryContext), nameof(CostBlockHistory.Context)));
        }
    }
}
