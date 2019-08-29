using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.Tests.DataAccessLayer.CostBlock.Entities;
using Gdc.Scd.Tests.DataAccessLayer.CostBlock.Impl;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.DataAccessLayer.CostBlock
{
    [TestFixture]
    public class CostBlockUpdateByCoordinateTest
    {
        private StandardKernel Ioc { get; set; }

        private DomainEnitiesMeta Meta { get; set; }

        private EntityFrameworkRepositorySet RepositorySet { get; set; }

        private ICostBlockRepository CostBlockRepository { get; set; }

        [OneTimeSetUp]
        public void Init()
        {
            this.Ioc = CreateIoc();
            this.Meta = Ioc.Get<DomainEnitiesMeta>();
            this.RepositorySet = Ioc.Get<EntityFrameworkRepositorySet>();
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

        [OneTimeTearDown]
        public void Cleanup()
        {
            this.RepositorySet.Database.EnsureDeleted();
        }

        [Test]
        public async Task SimpleInputLevelsTest()
        {
            var costBlockMeta = this.Meta.GetCostBlockEntityMeta("Application1", "CostBlock1");
            var rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(0, rowCount, "Wrong row count in cost block after creating");

            this.AddNamedIds<Dependency1>(10);
            this.AddNamedIds<SimpleInputLevel1>(10);
            this.AddNamedIds<SimpleInputLevel2>(10);
            this.AddNamedIds<SimpleInputLevel3>(10);

            this.CostBlockRepository.UpdateByCoordinates(costBlockMeta);

            rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(10 * 10 * 10 * 10, rowCount, $"Wrong row count in cost block after updating by coordinates");

            var queryInfo = new TestQueryInfo
            {
                CostBlockMeta = costBlockMeta,
                InputLevel = nameof(SimpleInputLevel2),
                InputLevelItemId = this.RepositorySet.GetRepository<SimpleInputLevel2>().GetAll().Select(item => item.Id).First(),
                CostElement = "SimpleCostElement",
                Value = 777
            };

            await this.UpdateCostBlock(queryInfo);

            rowCount = await this.InputLevelValueRowCount(queryInfo);

            Assert.AreEqual(10 * 10 * 10, rowCount, $"Wrong row count with specific value in cost block after updating values");

            this.AddNamedIds<Dependency1>(5, 11);
            this.AddNamedIds<SimpleInputLevel1>(5, 11);
            this.AddNamedIds<SimpleInputLevel2>(5, 11);
            this.AddNamedIds<SimpleInputLevel3>(5, 11);

            this.CostBlockRepository.UpdateByCoordinates(costBlockMeta);

            rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(15 * 15 * 15 * 15, rowCount, $"Wrong row count in cost block after updating by coordinates");

            rowCount = await this.InputLevelValueRowCount(queryInfo);

            Assert.AreEqual(10 * 10 * 15, rowCount, $"Wrong row count with specific value in cost block after updating by coordinates)");
        }

        [Test]
        public async Task RelatedInputLevelsTest()
        {
            var costBlockMeta = this.Meta.GetCostBlockEntityMeta("Application1", "CostBlock2");
            var rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(0, rowCount, "Wrong row count in cost block after creating");

            this.AddNamedIds<Dependency2>(10);
            this.AddNamedIds<RelatedInputLevel1>(
                10, 
                prepareAction: (inputLevel1, index1) =>
                {
                    inputLevel1.RelatedItems = this.BuildNamedIds<RelatedInputLevel2>(
                        10,
                        namePrefix: $"Test_{index1}_",
                        prepareAction: (inputLevel2, index2) =>
                        {
                            inputLevel2.RelatedItems = this.BuildNamedIds<RelatedInputLevel3>(10, namePrefix: $"Test_{index1}_{index2}_").ToList();
                        }).ToList();
                });

            this.CostBlockRepository.UpdateByCoordinates(costBlockMeta);

            rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(10 * 10 * 10 * 10, rowCount, $"Wrong row count in cost block after updating by coordinates");

            var relatedInputLevel2Item = this.RepositorySet.GetRepository<RelatedInputLevel2>().GetAll().Include(item => item.RelatedItems).First();

            var queryInfo = new TestQueryInfo
            {
                CostBlockMeta = costBlockMeta,
                InputLevel = nameof(RelatedInputLevel2),
                InputLevelItemId = relatedInputLevel2Item.Id,
                CostElement = "SimpleCostElement",
                Value = 777
            };

            await this.UpdateCostBlock(queryInfo);

            rowCount = await this.InputLevelValueRowCount(queryInfo);

            Assert.AreEqual(10 * 10, rowCount, $"Wrong row count with specific value in cost block after updating values");

            this.AddNamedIds<RelatedInputLevel1>(
                1,
                prepareAction: (inputLevel1, index1) =>
                {
                    inputLevel1.RelatedItems = this.BuildNamedIds<RelatedInputLevel2>(
                        1,
                        namePrefix: $"Test_{index1}_",
                        prepareAction: (inputLevel2, index2) =>
                        {
                            inputLevel2.RelatedItems = this.BuildNamedIds<RelatedInputLevel3>(10, namePrefix: $"Test_{index1}_{index2}_").ToList();
                        }).ToList();
                });

            this.CostBlockRepository.UpdateByCoordinates(costBlockMeta);

            rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(10 * 10 * 10 * 10 + 10 * 10, rowCount, $"Wrong row count in cost block after updating by coordinates");

            rowCount = await this.InputLevelValueRowCount(queryInfo);

            Assert.AreEqual(10 * 10, rowCount, $"Wrong row count with specific value in cost block after updating values");

            this.RepositorySet.GetRepository<RelatedInputLevel3>().Save(new RelatedInputLevel3
            {
                Name = "New test item",
                RelatedInputLevel2Id = relatedInputLevel2Item.Id
            });
            this.RepositorySet.Sync();

            this.CostBlockRepository.UpdateByCoordinates(costBlockMeta);

            rowCount = await this.RowCount(costBlockMeta);

            Assert.AreEqual(10 * 10 * 10 * 10 + 10 * 11, rowCount, $"Cost block must have {10 * 10 * 10 * 10 + 10 * 11} rows");

            rowCount = await this.InputLevelValueRowCount(queryInfo);

            Assert.AreEqual(10 * 11, rowCount, $"Cost block must have {10 * 11} rows");
        }

        private IEnumerable<T> BuildNamedIds<T>(
            int count = 10, 
            int start = 1, 
            Action<T, int> prepareAction = null, 
            string namePrefix = "Test") where T : NamedId, new()
        {
            var items = Enumerable.Range(start, count).Select(index => new T
            {
                Name = namePrefix + index
            });

            if (prepareAction != null)
            {
                items = items.Select((item, index) =>
                {
                    prepareAction(item, index);

                    return item;
                });
            }

            return items;
        }

        private void AddNamedIds<T>(
            int count = 10, 
            int start = 1,
            Action<T, int> prepareAction = null,
            string namePrefix = "Test") where T : NamedId, new()
        {
            var items = this.BuildNamedIds<T>(count, start, prepareAction, namePrefix);

            this.RepositorySet.GetRepository<T>().Save(items);
            this.RepositorySet.Sync();
        }

        private async Task<int> RowCount(BaseEntityMeta meta)
        {
            var countQuery = Sql.Select(SqlFunctions.Count()).From(meta);

            return await this.RepositorySet.ExecuteScalarAsync<int>(countQuery);
        }

        private async Task UpdateCostBlock(TestQueryInfo queryInfo)
        {
            await this.CostBlockRepository.Update(new[]
            {
                new EditInfo
                {
                    Meta = queryInfo.CostBlockMeta,
                    ValueInfos = new[]
                    {
                        new ValuesInfo
                        {
                            CoordinateFilter = new Dictionary<string, long[]>
                            {
                                [queryInfo.InputLevel] = new [] { queryInfo.InputLevelItemId }
                            },
                            Values = new Dictionary<string, object>
                            {
                                [queryInfo.CostElement] = queryInfo.Value
                            }
                        }
                    }
                }
            });
        }

        private async Task<int> InputLevelValueRowCount(TestQueryInfo queryInfo)
        {
            return await this.RepositorySet.ExecuteScalarAsync<int>($@"
                SELECT 
                    COUNT(*) 
                FROM 
                    [{queryInfo.CostBlockMeta.ApplicationId}].[{queryInfo.CostBlockMeta.Name}] 
                WHERE 
                    [{queryInfo.InputLevel}] = {queryInfo.InputLevelItemId} AND
                    [{queryInfo.CostElement}] = {queryInfo.Value}");
        }

        private class TestQueryInfo
        {
            public CostBlockEntityMeta CostBlockMeta { get; set; }

            public string InputLevel { get; set; }

            public long InputLevelItemId { get; set; }

            public string CostElement { get; set; }

            public object Value { get; set; }
        }
    }
}
