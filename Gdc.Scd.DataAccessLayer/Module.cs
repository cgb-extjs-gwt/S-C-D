using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
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

            services.AddTransient<BaseColumnMetaSqlBuilder<IdFieldMeta>, IdColumnMetaSqlBuilder>();
            services.AddTransient<BaseColumnMetaSqlBuilder<SimpleFieldMeta>, SimpleColumnMetaSqlBuilder>();
            services.AddTransient<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>, ReferenceColumnMetaSqlBuilder>();
            services.AddTransient<CreateTableMetaSqlBuilder>();
            services.AddTransient<DatabaseMetaSqlBuilder>();
            services.AddTransient<IConfigureApplicationHandler, DatabaseCreationHandler>();
            //services.AddTransient<ICustomConfigureTableHandler, ReactionTableConfigureHandler>();
        }
    }
}
