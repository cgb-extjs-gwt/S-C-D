﻿using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer
{
    public class Module : NinjectModule
    {
        public override void Load()
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
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InRequestScope();
            Bind<IRepositorySet, EntityFrameworkRepositorySet>().To<EntityFrameworkRepositorySet>().InRequestScope();
            Bind<ICostEditorRepository>().To<CostEditorRepository>().InRequestScope();
            Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InRequestScope();
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

            Kernel.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(HistoryContext), nameof(CostBlockHistory.Context)));
        }
    }
}
