using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Helpers;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using Gdc.Scd.MigrationTool.Helpers;
using Gdc.Scd.MigrationTool.Interfaces;
using Ninject;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Impl
{
    public class DataMigrator : IDataMigrator
    {
        private readonly IKernel serviceProvider;
        private readonly IRepositorySet repositorySet;
        private readonly ICostBlockRepository costBlockRepository;

        public DataMigrator(
            IKernel serviceProvider,
            IRepositorySet repositorySet,
            ICostBlockRepository costBlockRepository)
        {
            this.serviceProvider = serviceProvider;
            this.repositorySet = repositorySet;
            this.costBlockRepository = costBlockRepository;
        }

        public void AddCostElements(IEnumerable<CostElementInfo> costElementInfos)
        {
            var queries =
                costElementInfos.SelectMany(info => new[]
                                {
                                    BuildAlterCostBlockQuery(info),
                                    BuildAlterHistoryQuery(info)
                                })
                                .ToList();

            this.repositorySet.ExecuteSql(Sql.Queries(queries));

            SqlHelper BuildAlterCostBlockQuery(CostElementInfo costElementInfo)
            {
                var approvedCostElements =
                    costElementInfo.CostElementIds.Select(costElementInfo.Meta.GetApprovedCostElement)
                                                  .Select(field => field.Name);

                var newFields = costElementInfo.CostElementIds.Concat(approvedCostElements).ToArray();

                return this.BuildAddColumnsQuery(costElementInfo.Meta, newFields);
            }

            SqlHelper BuildAlterHistoryQuery(CostElementInfo costElementInfo)
            {
                return this.BuildAddColumnsQuery(costElementInfo.Meta.HistoryMeta, costElementInfo.CostElementIds);
            }
        }

        public void AddCostBlocks(IEnumerable<CostBlockEntityMeta> costBlocks, bool isAddingData)
        {
            var queries = costBlocks.Select(costBlock => this.BuildAddTableQuery(costBlock, isAddingData));

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }

        public void AddCostBlock(CostBlockEntityMeta costBlock, bool isAddingData)
        {
            var query = this.BuildAddTableQuery(costBlock, isAddingData);

            this.repositorySet.ExecuteSql(query);
        }

        public void AddColumn(BaseEntityMeta meta, string field)
        {
            var query = this.BuildAddColumnQuery(meta, field);

            this.repositorySet.ExecuteSql(query);
        }

        public void AddColumns(BaseEntityMeta meta, IEnumerable<string> fields)
        {
            var query = this.BuildAddColumnsQuery(meta, fields);

            this.repositorySet.ExecuteSql(query);
        }

        public void AddCalculatedColumn(string column, string table, string schema, ISqlBuilder calcQuery, bool isPersisted = false)
        {
            var persistedQuery = isPersisted ? "PERSISTED" : string.Empty;

            var query =
                MigratorSql.IfColumnNotExist(
                    column,
                    table,
                    schema,
                    new AlterTableSqlBuilder(table, schema)
                    {
                        Query = new AddColumnsSqlBuilder
                        {
                            Columns = new[]
                            {
                                new RawSqlBuilder($" ({calcQuery.Build(null)}) {persistedQuery}")
                            }
                        }
                    });

            this.repositorySet.ExecuteSql(query);
        }

        public void AddCalculatedColumn(string column, BaseEntityMeta meta, ISqlBuilder calcQuery, bool isPersisted = false)
        {
            this.AddCalculatedColumn(column, meta.Name, meta.Schema, calcQuery, isPersisted);
        }

        public void SplitCostBlock(CostBlockEntityMeta source, IEnumerable<CostBlockEntityMeta> targets, DomainEnitiesMeta enitiesMeta)
        {
            var sourceFields = source.AllFields.ToNamesArray();
            var sourceCoordFields = source.CoordinateFields.ToNamesArray();

            var queries = targets.SelectMany(BuildQueries);

            this.repositorySet.ExecuteSql(Sql.Queries(queries));

            IEnumerable<SqlHelper> BuildQueries(CostBlockEntityMeta target)
            {
                yield return this.BuildAddTableQuery(target, false);

                var actualVersionTempTable = $"#ActualVersion_{target.FullName}";

                yield return
                    Sql.If(
                        SqlOperators.NotExists(target),
                        Sql.Queries(new[]
                        {
                            BuildCreateActualVersionTempTableQuery(target, actualVersionTempTable),
                            BuildInsertActualVersionRowsQuery(target, actualVersionTempTable),
                            BuildInsertOtherRowsQuery(target, actualVersionTempTable)
                        }));

                yield return
                    Sql.If(
                        SqlOperators.NotExists(target.HistoryMeta),
                        BuildInsertHistoryQuery(target));
            }

            SqlHelper BuildCreateActualVersionTempTableQuery(CostBlockEntityMeta target, string actualVersionTempTable)
            {
                var groupByColumns = BuildGroupByColumns(target);
                var selectColumns = BuildSelectColumns(target.AllFields.WithoutIdField().ToNames(), groupByColumns);

                return
                    Sql.Select(selectColumns)
                       .Into(actualVersionTempTable)
                       .From(source)
                       .GroupBy(groupByColumns)
                       .Having(
                            SqlOperators.Greater(new CountSqlBuilder(), new ValueSqlBuilder(1)));
            }

            SqlHelper BuildInsertActualVersionRowsQuery(CostBlockEntityMeta target, string actualVersionTempTable)
            {
                var allFields = target.AllFields.ToNamesArray();

                var selectAcualIdsQuery =
                     Sql.Select(source.IdField.Name)
                        .From(actualVersionTempTable)
                        .Where(SqlOperators.IsNull(target.DeletedDateField.Name));

                var insertActualRowsQuery =
                    Sql.Insert(target, allFields)
                       .Query(
                            Sql.Select(allFields)
                               .From(source)
                               .Where(SqlOperators.In(source.IdField.Name, selectAcualIdsQuery)));

                var withoutIdFields = target.AllFields.WithoutIdField().ToNamesArray();

                var insertPreviousRowsQuery =
                    Sql.Insert(target, withoutIdFields)
                       .Query(
                            Sql.Select(withoutIdFields)
                               .From(actualVersionTempTable)
                               .Where(SqlOperators.IsNotNull(target.DeletedDateField.Name)));

                return Sql.Queries(new[]
                {
                    insertActualRowsQuery,
                    insertPreviousRowsQuery
                });
            }

            SqlHelper BuildInsertOtherRowsQuery(CostBlockEntityMeta target, string actualVersionTempTable)
            {
                var targetFields = target.AllFields.WithoutIdField().ToNamesArray();
                var groupFields = sourceCoordFields.Intersect(target.CoordinateFields.ToNames()).ToArray();
                var selectColumns = BuildSelectColumns(targetFields, groupFields);

                return
                    Sql.Insert(target, targetFields)
                       .Query(
                            Sql.Select(selectColumns)
                               .From(source)
                               .Where(
                                    ConditionHelper.Or(
                                        SqlOperators.NotExists(actualVersionTempTable),
                                        SqlOperators.IsNull(target.ActualVersionField.Name)))
                               .GroupBy(groupFields));
            }

            SqlHelper BuildInsertHistoryQuery(CostBlockEntityMeta target)
            {
                var historyFields = target.HistoryMeta.AllFields.ToNamesArray();
                var costBlockHistory = enitiesMeta.CostBlockHistory;

                var historyFilters =
                    enitiesMeta.CostBlocks.GetCostElementIdentifiers(target)
                                          .GroupBy(key => new { key.ApplicationId, key.CostBlockId })
                                          .Select(group => new Dictionary<string, IEnumerable<object>>
                                          {
                                              [costBlockHistory.ContextApplicationIdField.Name] = new[] { group.Key.ApplicationId },
                                              [costBlockHistory.ContextCostBlockIdField.Name] = new[] { group.Key.CostBlockId },
                                              [costBlockHistory.ContextCostElementIdField.Name] = group.Select(key => key.CostElementId).ToArray()
                                          })
                                          .Select(filter => new BracketsSqlBuilder
                                          {
                                              Query = ConditionHelper.AndStatic(filter).ToSqlBuilder()
                                          });

                var selectHistoriesQuery =
                    Sql.Select(target.HistoryMeta.CostBlockHistoryField.Name)
                       .From(enitiesMeta.CostBlockHistory)
                       .Where(ConditionHelper.Or(historyFilters));

                return
                    Sql.Insert(target.HistoryMeta, historyFields)
                       .Query(
                            Sql.Select(historyFields)
                               .From(source.HistoryMeta)
                               .Where(
                                    SqlOperators.In(
                                        target.HistoryMeta.CostBlockHistoryField.Name,
                                        selectHistoriesQuery)));

            }

            string[] BuildGroupByColumns(CostBlockEntityMeta target)
            {
                return
                    target.CoordinateFields.ConcatFields(target.ActualVersionField, target.CreatedDateField, target.DeletedDateField)
                                           .ToNamesArray();
            }

            BaseColumnInfo[] BuildSelectColumns(IEnumerable<string> selectFields, IEnumerable<string> groupFields)
            {
                return
                    selectFields.Select(
                                    field => 
                                        groupFields.Contains(field) 
                                            ? (BaseColumnInfo)new ColumnInfo(field) 
                                            : SqlFunctions.Min(field, alias: field))
                                .ToArray();
            }
        }

        private SqlHelper BuildAddTableQuery(CostBlockEntityMeta costBlock, bool isAddingData)
        {
            var costBlockTableQueries = new List<SqlHelper>
            {
                BuildCreateTableQuery(costBlock)
            };

            if (isAddingData)
            {
                costBlockTableQueries.Add(
                    this.costBlockRepository.BuildUpdateByCoordinatesQuery(costBlock));
            }

            return Sql.Queries(new[]
            {
                MigratorSql.IfTableNotExist(costBlock, costBlockTableQueries),
                MigratorSql.IfTableNotExist(
                    costBlock.HistoryMeta,
                    BuildCreateTableQuery(costBlock.HistoryMeta))
            });

            SqlHelper BuildCreateTableQuery(BaseEntityMeta meta)
            {
                var createTableBuilder = this.serviceProvider.Get<CreateTableMetaSqlBuilder>();
                createTableBuilder.Meta = meta;

                var queryList = new List<ISqlBuilder>
                {
                    createTableBuilder
                };

                queryList.AddRange(meta.AllFields.Select(field => new CreateColumnConstraintMetaSqlBuilder
                {
                    Meta = meta,
                    Field = field.Name
                }));

                return Sql.Queries(queryList);
            }
        }

        private SqlHelper BuildAddColumnQuery(BaseEntityMeta meta, string field)
        {
            var alterTableBuilder = this.serviceProvider.Get<AlterTableMetaSqlBuilder>();

            alterTableBuilder.Meta = meta;
            alterTableBuilder.NewFields = new[] { field };

            return MigratorSql.IfColumnNotExist(field, meta, alterTableBuilder);
        }

        private SqlHelper BuildAddColumnsQuery(BaseEntityMeta meta, IEnumerable<string> fields)
        {
            var queries = fields.Select(field => this.BuildAddColumnQuery(meta, field));

            return Sql.Queries(queries);
        }
    }
}
