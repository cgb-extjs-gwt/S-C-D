using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_01_10_14_55 : IMigrationAction
    {
        private readonly DomainEnitiesMeta meta;

        private readonly IRepositorySet repositorySet;

        public int Number => 777777777;

        public string Description => "Performance";

        public Migration_2020_01_10_14_55(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
            this.meta = meta;
        }

        public void Execute()
        {
            var queries = this.meta.RelatedItemsHistories.Select(BuildCreateRelatedItemsHistoryIndexQuery).ToList();

            queries.Add(BuildCreateCostBlockHistoryIndexQuery());

            foreach (var costBlockMeta in this.meta.CostBlocks)
            {
                queries.Add(BuildCreateCostBlockValueHistoryIndexQuery(costBlockMeta));
                queries.Add(BuildCreateCostBlockDeletedDateIndexQuery(costBlockMeta));
                queries.AddRange(BuildCreateCostEditorIndexQueries(costBlockMeta));
            }

            this.repositorySet.ExecuteSql(Sql.Queries(queries));

            string BuildIndexName(BaseEntityMeta meta, string fieldName)
            {
                return $"IX_{meta.Schema}_{meta.Name}_{fieldName}";
            }

            SqlHelper BuildCreateRelatedItemsHistoryIndexQuery(RelatedItemsHistoryEntityMeta meta)
            {
                return 
                    Sql.CreateIndexIfNotExists(
                        BuildIndexName(meta, meta.CostBlockHistoryField.Name),
                        meta,
                        new[] { new IndexColumn(meta.CostBlockHistoryField) });
            }

            SqlHelper BuildCreateCostBlockHistoryIndexQuery()
            {
                var indexName =
                    BuildIndexName(
                        this.meta.CostBlockHistory, 
                        this.meta.CostBlockHistory.ContextRegionInputIdField.Name);

                return
                    Sql.CreateIndexIfNotExists(
                        indexName,
                        this.meta.CostBlockHistory,
                        new[] { new IndexColumn(this.meta.CostBlockHistory.ContextRegionInputIdField) },
                        new[]
                        {
                            this.meta.CostBlockHistory.IdField,
                            this.meta.CostBlockHistory.EditDateField,
                            this.meta.CostBlockHistory.EditUserField,
                            this.meta.CostBlockHistory.ContextApplicationIdField,
                            this.meta.CostBlockHistory.ContextCostBlockIdField,
                            this.meta.CostBlockHistory.ContextCostElementIdField
                        });
            }

            SqlHelper BuildCreateCostBlockValueHistoryIndexQuery(CostBlockEntityMeta meta)
            {
                var includeFields =
                    meta.HistoryMeta.AllFields.Where(
                        field => 
                            field != meta.HistoryMeta.IdField && 
                            field != meta.HistoryMeta.CostBlockHistoryField);

                return
                    Sql.CreateIndexIfNotExists(
                        BuildIndexName(meta.HistoryMeta, meta.HistoryMeta.CostBlockHistoryField.Name),
                        meta.HistoryMeta,
                        new[] { new IndexColumn(meta.HistoryMeta.CostBlockHistoryField) },
                        includeFields);
            }

            SqlHelper BuildCreateCostBlockDeletedDateIndexQuery(CostBlockEntityMeta meta)
            {
                var includeFields =
                    meta.CoordinateFields.Concat(meta.CostElementsFields)
                                         .Concat(meta.CostElementsApprovedFields.Values);

                return
                    Sql.CreateIndexIfNotExists(
                        BuildIndexName(meta, meta.DeletedDateField.Name),
                        meta,
                        new[] { new IndexColumn(meta.DeletedDateField.Name) },
                        includeFields);
            }

            IEnumerable<SqlHelper> BuildCreateCostEditorIndexQueries(CostBlockEntityMeta meta)
            {
                var costElements = meta.DomainMeta.CostElements;
                var costElementGroups =
                    costElements.Where(costEl => costEl.RegionInput != null && costEl.Dependency != null)
                                .GroupBy(costEl => new
                                {
                                    RegionInput = costEl.RegionInput.Id,
                                    Dependency = costEl.Dependency.Id
                                });

                foreach (var group in costElementGroups)
                {
                    var columns = new[]
                    {
                        new IndexColumn(group.Key.RegionInput),
                        new IndexColumn(meta.DeletedDateField),
                        new IndexColumn(group.Key.Dependency)
                    };

                    var includeFields =
                        group.SelectMany(costEl => new[]
                             {
                                meta.CostElementsFields[costEl.Id],
                                meta.GetApprovedCostElement(costEl.Id)
                             });

                    yield return
                        Sql.CreateIndexIfNotExists(
                            BuildIndexName(meta, $"{group.Key.RegionInput}_{group.Key.Dependency}"),
                            meta,
                            columns,
                            includeFields);
                }
            }
        }
    }
}
