using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.Tests.Common.CostBlock;
using Gdc.Scd.Tests.Common.CostBlock.Constants;
using Gdc.Scd.Tests.Common.CostBlock.Entities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.Tests.DataAccessLayer.CostBlock
{
    [TestFixture]
    public class CostBlockUpdateByCoordinateTest : SetUpCostBlockTest
    {
        [OneTimeSetUp]
        public override void Init()
        {
            base.Init();
        }

        [OneTimeTearDown]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [Test]
        public async Task SimpleInputLevelsTest()
        {
            var costBlockMeta = this.Meta.CostBlocks[
                TestConstants.Application1Id,
                TestConstants.CostBlock1Id,
                TestConstants.SimpleCostElementId];

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
                CostElement = TestConstants.SimpleCostElementId,
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
            var costBlockMeta = this.Meta.CostBlocks[
                TestConstants.Application1Id,
                TestConstants.CostBlock2Id,
                TestConstants.SimpleCostElementId];

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
                CostElement = TestConstants.SimpleCostElementId,
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
                    [{queryInfo.CostBlockMeta.Schema}].[{queryInfo.CostBlockMeta.Name}] 
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
