using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.Tests.Common.CostBlock.Entities;
using Gdc.Scd.Tests.Common.CostBlock.Impl;
using Ninject;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;

namespace Gdc.Scd.Tests.Common.CostBlock
{
    public class SetUpCostBlockTest
    {
        private EntityFrameworkRepositorySet entityFrameworkRepositorySet;

        protected StandardKernel Ioc { get; set; }

        protected DomainEnitiesMeta Meta { get; set; }

        protected ICostBlockRepository CostBlockRepository { get; set; }

        protected IRepositorySet RepositorySet => this.entityFrameworkRepositorySet;

        public virtual void Init()
        {
            this.Ioc = CreateIoc();
            this.Meta = Ioc.Get<DomainEnitiesMeta>();
            this.entityFrameworkRepositorySet = Ioc.Get<EntityFrameworkRepositorySet>();
            this.CostBlockRepository = Ioc.Get<ICostBlockRepository>();

            this.Ioc.Get<IConfigureApplicationHandler>().Handle();

            StandardKernel CreateIoc()
            {
                var ioc = new StandardKernel();

                var domainMeta = GetDomainMeta();
                var domainEnitiesMeta = GetDomainEnitiesMeta();

                ioc.Bind<DomainMeta>().ToConstant(domainMeta);
                ioc.Bind<DomainEnitiesMeta>().ToConstant(domainEnitiesMeta);
                ioc.Bind<IRepositorySet, EntityFrameworkRepositorySet>().ToMethod(context => CreateRepositorySet()).InSingletonScope();
                ioc.Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InTransientScope();
                ioc.Bind<ICostBlockRepository>().To<CostBlockRepository>().InTransientScope();
                ioc.Bind<IConfigureApplicationHandler>().To<DatabaseCreationHandler>().InTransientScope();

                ioc.Bind<BaseColumnMetaSqlBuilder<IdFieldMeta>>().To<IdColumnMetaSqlBuilder>().InTransientScope();
                ioc.Bind<BaseColumnMetaSqlBuilder<SimpleFieldMeta>>().To<SimpleColumnMetaSqlBuilder>().InTransientScope();
                ioc.Bind<BaseColumnMetaSqlBuilder<ReferenceFieldMeta>>().To<ReferenceColumnMetaSqlBuilder>().InTransientScope();
                ioc.Bind<BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>>().To<CreatedDateTimeColumnMetaSqlBuilder>().InTransientScope();
                ioc.Bind<CreateTableMetaSqlBuilder>().To<CreateTableMetaSqlBuilder>().InTransientScope();
                ioc.Bind<DatabaseMetaSqlBuilder>().To<DatabaseMetaSqlBuilder>().InTransientScope();

                ioc.RegisterEntity<Dependency1>();
                ioc.RegisterEntity<Dependency2>();
                ioc.RegisterEntity<SimpleInputLevel1>();
                ioc.RegisterEntity<SimpleInputLevel2>();
                ioc.RegisterEntity<SimpleInputLevel3>();
                ioc.RegisterEntity<RelatedInputLevel1>();
                ioc.RegisterEntity<RelatedInputLevel2>();
                ioc.RegisterEntity<RelatedInputLevel3>();

                return ioc;

                DomainMeta GetDomainMeta()
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.CostBlock.TestDomainConfig.xml");

                    var domainMetaSevice = new DomainMetaSevice();

                    return domainMetaSevice.Get(stream);
                }

                DomainEnitiesMeta GetDomainEnitiesMeta()
                {
                    var domainEnitiesMetaService = new DomainEnitiesMetaService(new[]
                    {
                        new CoordinateEntityMetaProvider()
                    });

                    return domainEnitiesMetaService.Get(domainMeta);
                }

                EntityFrameworkRepositorySet CreateRepositorySet()
                {
                    var connectionStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["CommonDB"].ConnectionString);

                    connectionStringBuilder.InitialCatalog = connectionStringBuilder.InitialCatalog + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss");

                    return new EntityFrameworkRepositorySet(ioc, connectionStringBuilder.ConnectionString);
                }
            }
        }

        public virtual void Cleanup()
        {
            this.entityFrameworkRepositorySet.Database.EnsureDeleted();
        }
    }
}
