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

            AddCostBlockData();
            AddUser();

            void AddCostBlockData()
            {
                this.AddNamedIds<Dependency1>(10);
                this.AddNamedIds<SimpleInputLevel1>(10);
                this.AddNamedIds<SimpleInputLevel2>(10);
                this.AddNamedIds<SimpleInputLevel3>(10);

                var costBlockMeta = this.GetCostBlock1Meta();

                this.CostBlockRepository.UpdateByCoordinates(costBlockMeta);
            }

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
            var costBlockService = this.Ioc.Get<ICostBlockService>();
            var costBlockHistoryService = this.Ioc.Get<ICostBlockHistoryService>();
            var costBlockMeta = this.GetCostBlock1Meta();
            var coordinateFilter = BuildCoordinateFilter();
            var costElementContext = new CostElementContext
            {
                ApplicationId = costBlockMeta.Schema,
                CostBlockId = costBlockMeta.Name,
                CostElementId = TestConstants.SimpleCostElementId
            }; 

            var historyValues = new List<double>();

            for (var index = 0; index < 10; index++)
            {
                await Update(index);

                historyValues.Add(index);

                await CheckHistory(historyValues);
            }

            IDictionary<string, long[]> BuildCoordinateFilter()
            {
                var inputLevel1 = this.GetFirstItem<SimpleInputLevel1>();
                var inputLevel2 = this.GetFirstItem<SimpleInputLevel2>();
                var inputLevel3 = this.GetFirstItem<SimpleInputLevel3>();
                var dependency1 = this.GetFirstItem<Dependency1>();

                return new Dictionary<string, long[]>
                {
                    [TestConstants.SimpleInputLevel1Id] = new[] { inputLevel1.Id },
                    [TestConstants.SimpleInputLevel2Id] = new[] { inputLevel2.Id },
                    [TestConstants.SimpleInputLevel3Id] = new[] { inputLevel3.Id },
                    [TestConstants.Dependency1Id] = new[] { dependency1.Id },
                };
            }

            async Task Update(double value)
            {
                var values = new Dictionary<string, object>
                {
                    [costElementContext.CostElementId] = value
                };

                var editInfos = new EditInfo[]
                {
                    new EditInfo
                    {
                        Meta = costBlockMeta,
                        ValueInfos = new ValuesInfo[]
                        {
                            new ValuesInfo
                            {
                                CoordinateFilter = coordinateFilter,
                                Values = values
                            }
                        }
                    }
                };

                await costBlockService.Update(
                    editInfos, 
                    new ApprovalOption
                    {
                        IsApproving = false
                    }, 
                    EditorType.Test);
            }

            async Task CheckHistory(List<double> values)
            {
                var data = await costBlockHistoryService.GetHistory(costElementContext, coordinateFilter);
                var history = data.Items.Reverse().ToArray();

                Assert.AreEqual(values.Count, history.Length, "Wrong count history");

                for (var index = 0; index < values.Count; index++)
                {
                    Assert.AreEqual(values[index], history[index].Value, $"Wrong history value. Index {index}");
                }
            }
        }

        protected override void InitIoc(StandardKernel ioc)
        {
            base.InitIoc(ioc);

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

            ioc.RegisterEntity<User>();
            ioc.RegisterEntity<Currency>();
            ioc.RegisterEntity<CostBlockHistory>(builder => builder.OwnsOne(typeof(CostElementContext), nameof(CostBlockHistory.Context)));
        }

        private CostBlockEntityMeta GetCostBlock1Meta()
        {
            return this.Meta.GetCostBlockEntityMeta(TestConstants.Application1Id, TestConstants.CostBlock1Id);
        }
    }
}
