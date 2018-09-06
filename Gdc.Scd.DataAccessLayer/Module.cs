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
            services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
            services.AddScoped<EntityFrameworkRepositorySet>();
            services.AddScoped<IRepositorySet>(serviceProvider => serviceProvider.GetService<EntityFrameworkRepositorySet>());
            services.AddScoped<ISqlRepository, SqlRepository>();
            services.AddScoped<ICostEditorRepository, CostEditorRepository>();
            services.AddScoped<ICostBlockValueHistoryRepository, CostBlockValueHistoryRepository>();
            services.AddScoped<IRepository<CostBlockHistory>, CostBlockHistoryRepository>();
            services.AddScoped<IRepository<ReactionTimeType>, ReactionTimeTypeRepository>();
            services.AddScoped<IRepository<ReactionTimeAvalability>, ReactionTimeAvalabilityRepository>();
            services.AddScoped<IRepository<ReactionTimeTypeAvalability>, ReactionTimeTypeAvalabilityRepository>();
            services.AddScoped<ICostBlockValueHistoryQueryBuilder, CostBlockValueHistoryQueryBuilder>();
            services.AddScoped<IRepository<YearAvailability>, YearAvailabilityRepository>();
            services.AddScoped<IQualityGateRepository, QualityGateRepository>();
            services.AddScoped<IQualityGateQueryBuilder, QualityGateQueryBuilder>();

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
