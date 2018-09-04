using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.DataAccessLayer
{
    public class Module : IModule
    {
        public void Init(IServiceCollection services)
        {
            services.AddTransient(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
            services.AddTransient<EntityFrameworkRepositorySet>();
            services.AddTransient<IRepositorySet>(serviceProvider => serviceProvider.GetService<EntityFrameworkRepositorySet>());
            services.AddTransient<ISqlRepository, SqlRepository>();
            services.AddTransient<ICostEditorRepository, CostEditorRepository>();
            services.AddTransient<ICostBlockValueHistoryRepository, CostBlockValueHistoryRepository>();
            services.AddTransient<IRepository<CostBlockHistory>, CostBlockHistoryRepository>();
            services.AddTransient<IRepository<ReactionTimeType>, ReactionTimeTypeRepository>();
            services.AddTransient<IRepository<ReactionTimeAvalability>, ReactionTimeAvalabilityRepository>();
            services.AddTransient<IRepository<ReactionTimeTypeAvalability>, ReactionTimeTypeAvalabilityRepository>();

            services.AddTransient<BaseColumnMetaSqlBuilder<IdFieldMeta>, IdColumnMetaSqlBuilder>();
            services.AddTransient<BaseColumnMetaSqlBuilder<SimpleFieldMeta>, SimpleColumnMetaSqlBuilder>();
            services.AddTransient<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>, ReferenceColumnMetaSqlBuilder>();
            services.AddTransient<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>, CreatedDateTimeColumnMetaSqlBuilder>();
            services.AddTransient<CreateTableMetaSqlBuilder>();
            services.AddTransient<DatabaseMetaSqlBuilder>();
            services.AddTransient<IConfigureApplicationHandler, DatabaseCreationHandler>();
            services.AddTransient<IConfigureDatabaseHandler, ViewConfigureHandler>();
            services.AddTransient<ICustomConfigureTableHandler, ViewConfigureHandler>();

            services.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(HistoryContext), nameof(CostBlockHistory.Context)));
        }
    }
}
