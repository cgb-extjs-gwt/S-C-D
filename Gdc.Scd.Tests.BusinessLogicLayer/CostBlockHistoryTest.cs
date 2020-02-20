using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Tests.Common.CostBlock;
using Gdc.Scd.Tests.Common.CostBlock.Constants;
using Gdc.Scd.Tests.Common.CostBlock.Entities;
using Gdc.Scd.Tests.Common.Entities;
using Gdc.Scd.Tests.Common.Impl;
using Ninject;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.Tests.BusinessLogicLayer
{
    [TestFixture]
    public class CostBlockHistoryTest : SetUpCostBlockTest
    {
        [SetUp]
        public override void Init()
        {
            base.Init();

            AddUser();

            void AddUser()
            {
                const string TestUserLogin = "test";

                this.Ioc.Get<IUserService>().Save(new User
                {
                    Name = TestUserLogin,
                    Login = TestUserLogin,
                    Email = TestUserLogin,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            Role = new Role
                            {
                                Name = RoleConstants.ScdAdmin,
                                RolePermissions = new List<RolePermission>
                                {
                                    new RolePermission
                                    {
                                        Permission = new Permission
                                        {
                                            Name = PermissionConstants.Admin
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

                FakePrincipalProvider.CurrentPrincipal = new Principal
                {
                    Identity = new Identity
                    {
                        IsAuthenticated = true,
                        Name = TestUserLogin
                    }
                };
            }
        }

        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [Test]
        public async Task SimpleHistoryTest()
        {
            this.AddDataToCostBlock1(TestConstants.SimpleCostElementId);

            var historyChecker = this.Ioc.Get<HistoryChecker>();
            var costBlockMeta = this.Meta.CostBlocks[
                TestConstants.Application1Id, 
                TestConstants.CostBlock1Id,
                TestConstants.SimpleCostElementId];

            historyChecker.CostBlockMeta = costBlockMeta;
            historyChecker.CostElementContext = new CostElementContext
            {
                ApplicationId = costBlockMeta.Schema,
                CostBlockId = costBlockMeta.Name,
                CostElementId = TestConstants.SimpleCostElementId
            };
            historyChecker.CoordinateFilter = new Dictionary<string, long[]>
            {
                [TestConstants.Dependency1Id] = new[] { this.GetFirstItem<Dependency1>().Id },
                [TestConstants.SimpleInputLevel1Id] = new[] { this.GetFirstItem<SimpleInputLevel1>().Id },
                [TestConstants.SimpleInputLevel2Id] = new[] { this.GetFirstItem<SimpleInputLevel2>().Id },
                [TestConstants.SimpleInputLevel3Id] = new[] { this.GetFirstItem<SimpleInputLevel3>().Id }
            };
            
            await historyChecker.UpdateAndCheck();

            historyChecker.CoordinateFilter[TestConstants.SimpleInputLevel3Id][0] = this.GetItemByIndex<SimpleInputLevel3>(1).Id;

            await historyChecker.UpdateAndCheck();
        }

        [Test]
        public async Task DifferentInputLevelsHistoryTest()
        {
            this.AddDataToCostBlock1(TestConstants.SimpleCostElementId);

            var historyChecker = this.Ioc.Get<HistoryChecker>();
            var costBlockMeta = this.Meta.CostBlocks[
                TestConstants.Application1Id, 
                TestConstants.CostBlock1Id,
                TestConstants.SimpleCostElementId];

            historyChecker.CostBlockMeta = costBlockMeta;
            historyChecker.CostElementContext = new CostElementContext
            {
                ApplicationId = costBlockMeta.Schema,
                CostBlockId = costBlockMeta.Name,
                CostElementId = TestConstants.SimpleCostElementId
            };
            historyChecker.CoordinateFilter = new Dictionary<string, long[]>
            {
                [TestConstants.Dependency1Id] = new[] { this.GetFirstItem<Dependency1>().Id },
                [TestConstants.SimpleInputLevel1Id] = new[] { this.GetFirstItem<SimpleInputLevel1>().Id }
            };

            var level1Values = await historyChecker.UpdateAndCheck(3);

            historyChecker.CoordinateFilter.Add(TestConstants.SimpleInputLevel2Id, new[] { this.GetFirstItem<SimpleInputLevel2>().Id });

            var level2Values = await historyChecker.UpdateAndCheck(3, level1Values);

            historyChecker.CoordinateFilter.Add(TestConstants.SimpleInputLevel3Id, new[] { this.GetFirstItem<SimpleInputLevel3>().Id });

            await historyChecker.UpdateAndCheck(3, level1Values.Concat(level2Values));

            historyChecker.CoordinateFilter[TestConstants.SimpleInputLevel3Id][0] = this.GetItemByIndex<SimpleInputLevel3>(1).Id;

            await historyChecker.UpdateAndCheck(3, level1Values.Concat(level2Values));
        }

        [Test]
        public async Task ChangeCoordinatesHistoryTest()
        {
            this.AddDataToCostBlock2(TestConstants.SimpleCostElementId);

            var dependency2Item = this.GetFirstItem<Dependency2>();
            var relatedInputLevel3Service = this.Ioc.Get<IDomainService<RelatedInputLevel3>>();
            var relatedInputLevel3Items = 
                relatedInputLevel3Service.GetAll()
                                         .Take(2)
                                         .Include(item => item.RelatedInputLevel2)
                                         .ToArray();

            var oldRelatedInputLevel2Ids = relatedInputLevel3Items.Select(item => item.RelatedInputLevel2Id).ToArray();
            var newRelatedInputLevel2Ids =
                this.Ioc.Get<IDomainService<RelatedInputLevel2>>()
                        .GetAll()
                        .Where(item => !oldRelatedInputLevel2Ids.Contains(item.Id))
                        .Take(2)
                        .Select(item => item.Id)
                        .ToArray();

            await CheckHistoryByChangingCoordinate(dependency2Item.Id, relatedInputLevel3Items[0], newRelatedInputLevel2Ids[0]);
            await CheckHistoryByChangingCoordinate(dependency2Item.Id, relatedInputLevel3Items[1], newRelatedInputLevel2Ids[1]);

            async Task CheckHistoryByChangingCoordinate(
                long dependency2id, 
                RelatedInputLevel3 relatedInputLevel3Item, 
                long newRelatedInputLevel2Id)
            {
                var historyChecker = this.Ioc.Get<HistoryChecker>();
                var costBlockMeta = this.Meta.CostBlocks[
                    TestConstants.Application1Id, 
                    TestConstants.CostBlock2Id, 
                    TestConstants.SimpleCostElementId];

                historyChecker.CostBlockMeta = costBlockMeta;
                historyChecker.CostElementContext = new CostElementContext
                {
                    ApplicationId = costBlockMeta.Schema,
                    CostBlockId = costBlockMeta.Name,
                    CostElementId = TestConstants.SimpleCostElementId
                };

                var oldCoordinates = new Dictionary<string, long>
                {
                    [TestConstants.RelatedInputLevel2Id] = relatedInputLevel3Item.RelatedInputLevel2Id,
                    [TestConstants.RelatedInputLevel3Id] = relatedInputLevel3Item.Id
                };

                historyChecker.CoordinateFilter =
                    new Dictionary<string, long[]>(oldCoordinates.ToDictionary(x => x.Key, x => new[] { x.Value }))
                    {
                        [TestConstants.Dependency2Id] = new[] { dependency2id },
                        [TestConstants.RelatedInputLevel1Id] = new[] { relatedInputLevel3Item.RelatedInputLevel2.RelatedInputLevel1Id }
                    };

                var historyValues = await historyChecker.UpdateAndCheck();

                relatedInputLevel3Item.RelatedInputLevel2Id = newRelatedInputLevel2Id;

                relatedInputLevel3Service.Save(relatedInputLevel3Item);

                await this.CostBlockRepository.UpdateByCoordinatesAsync(
                    costBlockMeta,
                    new[]
                    {
                    new UpdateQueryOption(
                        oldCoordinates,
                        new Dictionary<string, long>(oldCoordinates)
                        {
                            [TestConstants.RelatedInputLevel2Id] = relatedInputLevel3Item.RelatedInputLevel2Id
                        })
                    });

                historyChecker.CoordinateFilter[TestConstants.RelatedInputLevel2Id][0] = relatedInputLevel3Item.RelatedInputLevel2Id;

                await historyChecker.UpdateAndCheck(3, historyValues);
            }
        }

        protected override void InitIoc(StandardKernel ioc)
        {
            base.InitIoc(ioc);

            ioc.Bind(typeof(IDomainService<>)).To(typeof(DomainService<>)).InTransientScope();
            ioc.Bind<ICostBlockService>().To<CostBlockService>().InTransientScope();
            ioc.Bind<IUserService>().To<UserService>().InTransientScope();
            ioc.Bind<IQualityGateSevice>().To<QualityGateSevice>().InTransientScope();
            ioc.Bind<ICostBlockHistoryService>().To<CostBlockHistoryService>().InTransientScope();
            ioc.Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InTransientScope();
            ioc.Bind<ICostBlockValueHistoryRepository>().To<CostBlockValueHistoryRepository>().InTransientScope();
            ioc.Bind<ICostBlockValueHistoryQueryBuilder>().To<CostBlockValueHistoryQueryBuilder>().InTransientScope();
            ioc.Bind<IUserRepository>().To<UserRepository>().InTransientScope();
            ioc.Bind<ISqlRepository>().To<SqlRepository>().InTransientScope();
            ioc.Bind<IQualityGateRepository>().To<QualityGateRepository>().InTransientScope();
            ioc.Bind<IQualityGateQueryBuilder>().To<QualityGateQueryBuilder>().InTransientScope();
            ioc.Bind<HistoryChecker>().To<HistoryChecker>().InTransientScope();

            ioc.RegisterEntity<User>();
            ioc.RegisterEntity<Currency>();
            ioc.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(CostElementContext), nameof(CostBlockHistory.Context)));
        }

        private void AddDataToCostBlock1(string costElementId)
        {
            this.AddNamedIds<Dependency1>(10);
            this.AddNamedIds<SimpleInputLevel1>(10);
            this.AddNamedIds<SimpleInputLevel2>(10);
            this.AddNamedIds<SimpleInputLevel3>(10);

            var costBlock = this.Meta.CostBlocks[TestConstants.Application1Id, TestConstants.CostBlock1Id, costElementId];
            
            this.CostBlockRepository.UpdateByCoordinates(costBlock);
        }

        private void AddDataToCostBlock2(string costElementId)
        {
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
                            inputLevel2.RelatedItems =
                                this.BuildNamedIds<RelatedInputLevel3>(10, namePrefix: $"Test_{index1}_{index2}_").ToList();
                        }).ToList();
                });

            var costBlock =
                this.Meta.CostBlocks[TestConstants.Application1Id, TestConstants.CostBlock2Id, costElementId];

            this.CostBlockRepository.UpdateByCoordinates(costBlock);
        }

        private class HistoryChecker
        {
            private readonly ICostBlockService costBlockService;

            private readonly ICostBlockHistoryService costBlockHistoryService;

            public HistoryChecker(
                ICostBlockService costBlockService, 
                ICostBlockHistoryService costBlockHistoryService)
            {
                this.costBlockService = costBlockService;
                this.costBlockHistoryService = costBlockHistoryService;
            }

            public CostBlockEntityMeta CostBlockMeta { get; set; }

            public CostElementContext CostElementContext { get; set; }

            public IDictionary<string, long[]> CoordinateFilter { get; set; }

            public async Task Update(double value)
            {
                var values = new Dictionary<string, object>
                {
                    [this.CostElementContext.CostElementId] = value
                };

                var editInfos = new EditInfo[]
                {
                    new EditInfo
                    {
                        Meta = this.CostBlockMeta,
                        ValueInfos = new ValuesInfo[]
                        {
                            new ValuesInfo
                            {
                                CoordinateFilter = this.CoordinateFilter,
                                Values = values
                            }
                        }
                    }
                };

                await this.costBlockService.Update(
                    editInfos,
                    new ApprovalOption
                    {
                        IsApproving = false
                    },
                    EditorType.Test);
            }

            public async Task CheckHistory(List<double> values)
            {
                var data = await this.costBlockHistoryService.GetHistory(this.CostElementContext, this.CoordinateFilter);
                var history = data.Items.Reverse().ToArray();

                Assert.AreEqual(values.Count, history.Length, "Wrong count history");

                for (var index = 0; index < values.Count; index++)
                {
                    Assert.AreEqual(values[index], history[index].Value, $"Wrong history value. Index {index}");
                }
            }

            public async Task<List<double>> UpdateAndCheck(
                int countValues = 10, 
                IEnumerable<double> previousHistoryValues = null)
            {
                var updateValues = new List<double>();
                var historyValues = new List<double>(previousHistoryValues ?? Enumerable.Empty<double>());

                for (var index = 0; index < countValues; index++)
                {
                    await this.Update(index);

                    updateValues.Add(index);
                    historyValues.Add(index);

                    await this.CheckHistory(historyValues);
                }

                return updateValues;
            }
        }
    }
}
