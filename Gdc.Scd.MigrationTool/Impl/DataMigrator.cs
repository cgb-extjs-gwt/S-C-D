using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Helpers;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
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
using System.Data;
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
            var queries = costBlocks.Select(costBlock => this.BuildAddTablesQuery(costBlock, isAddingData));

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }

        public void AddCostBlock(CostBlockEntityMeta costBlock, bool isAddingData)
        {
            var query = this.BuildAddTablesQuery(costBlock, isAddingData);

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

        public void SplitCostBlock(CostBlockEntityMeta source, IEnumerable<CostBlockEntityMeta> targets, DomainEnitiesMeta enitiesMeta, bool dropSource = false)
        {
            var sourceFields = source.AllFields.ToNamesArray();
            var sourceCoordFields = source.CoordinateFields.ToNamesArray();
            var queries = targets.Select(BuildQuery);

            this.repositorySet.ExecuteSql(Sql.Queries(queries));

            if (dropSource)
            {
                this.DropCostBlock(source);
            }

            SqlHelper BuildQuery(CostBlockEntityMeta target)
            {
                return 
                    this.BuildAddTablesQuery(
                        target, 
                        false,
                        Sql.Queries(new[]
                        {
                            new SetIdentityInsertSqlBuilder(target, true),
                            BuildInsertActualVersionRowsQuery(target).ToSqlBuilder(),
                            BuildInsertPreviousVersionRowsQuery(target).ToSqlBuilder(),
                            new SetIdentityInsertSqlBuilder(target, false),
                            BuildInsertOtherRowsQuery(target).ToSqlBuilder()
                        }),
                        BuildInsertHistoryQuery(target));
            }

            SqlHelper BuildInsertActualVersionRowsQuery(CostBlockEntityMeta target)
            {
                var groupByColumns =
                    target.CoordinateFields.ConcatFields(target.DeletedDateField)
                                           .ToNamesArray();

                var fields = target.AllFields.WithoutIdField().WithoutTypeField<ComputedFieldMeta>();
                var selectColumns = BuildSelectColumns(fields, groupByColumns);

                var idsQuery =
                    Sql.Select(SqlFunctions.Min(target.IdField))
                       .From(source)
                       .Where(SqlOperators.Equals(new ColumnInfo(target.IdField), new ColumnInfo(target.ActualVersionField)))
                       .GroupBy(groupByColumns);

                var allFields = target.AllFields.WithoutTypeField<ComputedFieldMeta>().ToNamesArray();

                return
                    Sql.Insert(target, allFields)
                       .Query(
                            Sql.Select(allFields)
                               .From(source)
                               .Where(SqlOperators.In(source.IdField.Name, idsQuery)));
            }

            SqlHelper BuildInsertPreviousVersionRowsQuery(CostBlockEntityMeta target)
            {
                var allFields = target.AllFields.WithoutTypeField<ComputedFieldMeta>().ToNamesArray();

                var selectActualIdsQuery =
                    Sql.Select(target.ActualVersionField.Name)
                       .From(target, isolationLevel: IsolationLevel.ReadUncommitted);

                return
                    Sql.Insert(target, allFields)
                       .Query(
                            Sql.Select(allFields)
                               .From(source)
                               .Where(
                                    ConditionHelper.And(
                                        SqlOperators.In(source.ActualVersionField.Name, selectActualIdsQuery),
                                        SqlOperators.NotEquals(
                                            new ColumnInfo(source.IdField),
                                            new ColumnInfo(source.ActualVersionField)))));

            }

            SqlHelper BuildInsertOtherRowsQuery(CostBlockEntityMeta target)
            {
                var targetFields = target.AllFields.WithoutIdField().WithoutTypeField<ComputedFieldMeta>().ToArray();
                var groupFields = 
                    target.CoordinateFields.ConcatFields(target.DeletedDateField)
                                           .ToNamesArray();

                var selectColumns = BuildSelectColumns(targetFields, groupFields);

                return
                    Sql.Insert(target, targetFields.ToNamesArray())
                       .Query(
                            Sql.Select(selectColumns)
                               .From(source)
                               .Where(SqlOperators.IsNull(target.ActualVersionField.Name))
                               .GroupBy(groupFields));
            }

            SqlHelper BuildInsertHistoryQuery(CostBlockEntityMeta target)
            {
                var historyFields = target.HistoryMeta.AllFields.WithoutIdField().ToNamesArray();
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

            BaseColumnInfo[] BuildSelectColumns(IEnumerable<FieldMeta> selectFields, IEnumerable<string> groupFields)
            {
                return
                    selectFields.Select(
                                    field => 
                                        groupFields.Contains(field.Name) 
                                            ? (BaseColumnInfo)new ColumnInfo(field) 
                                            : SqlFunctions.Min(field, alias: field.Name))
                                .ToArray();
            }
        }

        public void CreateCostBlockView(
            string shema, 
            string name, 
            IEnumerable<CostBlockEntityMeta> costBlocks, 
            BaseColumnInfo[] additionalColumns = null,
            string[] ignoreCoordinates = null)
        {
            var costBlockArray = 
                costBlocks.OrderByDescending(costBlock => costBlock.CoordinateFields.Count())
                          .Distinct()
                          .ToArray();

            var coordinateColumns =
                costBlockArray.SelectMany(costBlock => GetCoordinates(costBlock).Select(field => new { costBlock, field }))
                              .GroupBy(info => info.field.Name)
                              .Select(group => new ColumnInfo(group.Key, group.First().costBlock.Name));

            var costElementColumns =
                costBlockArray.SelectMany(
                    costBlock => 
                        costBlock.AllCostElemetFields.OrderBy(field => field.Name)
                                                     .Select(field => new ColumnInfo(field, costBlock)));

            IEnumerable<BaseColumnInfo> columns = coordinateColumns.Concat(costElementColumns);

            if (additionalColumns != null)
            {
                columns = columns.Concat(additionalColumns);
            }

            var fromCostBlock = costBlockArray[0];
            var query = Sql.Select(columns.ToArray()).From(fromCostBlock);

            var joinFields = GetCoordinates(fromCostBlock).Select(field => field.Name).ToArray();

            foreach (var costBlock in costBlockArray)
            {
                joinFields = joinFields.Intersect(GetCoordinates(costBlock).Select(field => field.Name)).ToArray();
            }

            foreach (var costBlock in costBlockArray.Skip(1))
            {
                var conditions =
                    joinFields.Select(
                        field => SqlOperators.Equals(new ColumnInfo(field, fromCostBlock.Name), new ColumnInfo(field, costBlock.Name)));

                query = query.Join(costBlock, ConditionHelper.And(conditions));
            }

            var notDeletedConditions =
                costBlockArray.Select(costBlock => CostBlockQueryHelper.BuildNotDeletedCondition(costBlock, costBlock.Name));

            var sqlBuilder = query.Where(ConditionHelper.And(notDeletedConditions)).ToSqlBuilder();

            var createViewQuery = new CreateViewSqlBuilder
            {
                Shema = shema,
                Name = name,
                Query = sqlBuilder
            };

            this.repositorySet.ExecuteSql(new SqlHelper(createViewQuery));

            IEnumerable<ReferenceFieldMeta> GetCoordinates(CostBlockEntityMeta costBlock)
            {
                var fields = costBlock.CoordinateFields;

                if (ignoreCoordinates != null)
                {
                    fields = fields.Where(field => !ignoreCoordinates.Contains(field.Name));
                }

                return fields;
            }
        }

        public void DropTable(string tableName, string schema)
        {
            var query = MigratorSql.IfTableExist(tableName, schema, Sql.DropTable(tableName, schema));

            this.repositorySet.ExecuteSql(query);
        }

        public void DropTable(BaseEntityMeta meta)
        {
            this.DropTable(meta.Name, meta.Schema);
        }

        public void DropCostBlock(CostBlockEntityMeta costBlock)
        {
            this.DropTable(costBlock);
            this.DropTable(costBlock.HistoryMeta);
        }

        public void DropView(string viewName, string schema)
        {
            var query = new SqlHelper(new DropViewSqlBuilder
            {
                Name = viewName,
                Schema = schema,
                IfExists = true
            });

            this.repositorySet.ExecuteSql(query);
        }

        private SqlHelper BuildAddTablesQuery(
            CostBlockEntityMeta costBlock, 
            bool isAddingData, 
            SqlHelper additionalCostBlockQuery = null,
            SqlHelper additionalHistoryQuery = null)
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

            if (additionalCostBlockQuery != null)
            {
                costBlockTableQueries.Add(additionalCostBlockQuery);
            }

            var historyTableQueries = new List<SqlHelper>
            {
                BuildCreateTableQuery(costBlock.HistoryMeta)
            };

            if (additionalHistoryQuery != null)
            {
                historyTableQueries.Add(additionalHistoryQuery);
            }

            return Sql.Queries(new[]
            {
                MigratorSql.IfTableNotExist(costBlock, costBlockTableQueries),
                MigratorSql.IfTableNotExist(costBlock.HistoryMeta, historyTableQueries)
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

            var columnConstrainBuilder = this.serviceProvider.Get<CreateColumnConstraintMetaSqlBuilder>();

            columnConstrainBuilder.Meta = meta;
            columnConstrainBuilder.Field = field;

            var query = Sql.Queries(new ISqlBuilder[] 
            {
                alterTableBuilder,
                columnConstrainBuilder
            });

            return MigratorSql.IfColumnNotExist(field, meta, query);
        }

        private SqlHelper BuildAddColumnsQuery(BaseEntityMeta meta, IEnumerable<string> fields)
        {
            var queries = fields.Select(field => this.BuildAddColumnQuery(meta, field));

            return Sql.Queries(queries);
        }
    }
}
