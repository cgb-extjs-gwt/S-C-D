using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class QualityGateQueryBuilder : IQualityGateQueryBuilder
    {
        private const string NewValueColumn = "NewValue";

        private const string OldValueColumn = "OldValue";

        private const string NewValuesTable = "NewValues";

        private const string CountryGroupAvgColumn = "CountryGroupAvg";

        private const string InnerQualityGateTable = "InnerQualityGateTable";

        private const string ResultQualityGateTable = "ResultQualityGateTable";

        private const string InnerQualityGateCostBlockTable = "InnerQualityGateCostBlockTable";

        private const string InnerQualityGateCountryTable = "InnerQualityGateCountryTable";

        private const string CountryGroupCheckColumn = "CountryGroupCheck";

        private const string CountryGroupCoeffParam = "сountryGroupCoeff";

        private const string PeriodCheckColumn = "PeriodCheck";

        private const string PeriodCoeffParam = "periodCoeff";

        private const string CountryGroupAverageTable = "CountryGroupAverageTable";

        private const string CountryGroupAverageColumn = "CountryGroupAverageColumn";

        private const string CountryTable = "CountryTable";

        private readonly string countryGroupIdColumnName = $"{nameof(CountryGroup)}{nameof(CountryGroup.Id)}";

        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ICostBlockValueHistoryQueryBuilder historyQueryBuilder;

        public QualityGateQueryBuilder(ICostBlockValueHistoryQueryBuilder historyQueryBuilder, DomainMeta domainMeta, DomainEnitiesMeta domainEnitiesMeta)
        {
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.historyQueryBuilder = historyQueryBuilder;
        }

        public SqlHelper BuildQualityGateQuery(
            HistoryContext historyContext,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var costBlockJoinCondition =
                SqlOperators.Equals(
                    new ColumnInfo(historyContext.InputLevelId, NewValuesTable),
                    new ColumnInfo(historyContext.InputLevelId, InnerQualityGateCostBlockTable))
                            .And(SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, InnerQualityGateCostBlockTable));

            var innerQualityGateQuery =
                this.BuildSelectInnerQualityGateQuery(historyContext)
                    .FromQuery(this.BuildNewValuesQuery(editItems, historyContext.InputLevelId), NewValuesTable)
                    .Join(costBlockMeta, costBlockJoinCondition, aliasMetaTable: InnerQualityGateCostBlockTable);

            innerQualityGateQuery = this.BuildJoinCountryInnerQualityGateQuery(costBlockMeta, innerQualityGateQuery);

            var selectCountryGroupAverageTableQuery = this.BuildSelectCountryGroupAverageTableQuery(historyContext);
            var joinHistoryValueQueryOptions = new JoinHistoryValueQueryOptions
            {
                InputLevelJoinType = InputLevelJoinType.All,
                JoinInfos = this.BuildCountryGroupAverageTableJoinInfos(costBlockMeta)
            };
            var countryGroupAverageTableQuery = 
                this.historyQueryBuilder.BuildJoinHistoryValueQuery(historyContext, selectCountryGroupAverageTableQuery, joinHistoryValueQueryOptions)
                                        .Where(this.BuildWhereCountryGroupAverageTableQuery(costBlockMeta, costBlockFilter));

            var options = new QualityGateQueryOptions
            {
                InnerQualityGateQuery = innerQualityGateQuery.Where(costBlockFilter, InnerQualityGateCostBlockTable),
                CountryGroupAverageTableQuery = this.BuildGroupByCountryGroupAverageTableQuery(historyContext, countryGroupAverageTableQuery)
            };

            return this.BuildQualityGateQuery(historyContext, options);
        }

        public SqlHelper BuildQulityGateHistoryQuery(
            CostBlockHistory history, 
            long? historyValueId = null,
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            const string GroupedByInputLevelTable = "GroupedByInputLevelTable";
            const string ApproveHistoryValueTable = "ApproveHistoryValueTable";

            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var innerQualityGateQuery =
                this.BuildSelectInnerQualityGateQuery(history.Context)
                    .From(costBlockMeta);

            innerQualityGateQuery = this.BuildJoinCountryInnerQualityGateQuery(costBlockMeta, innerQualityGateQuery);

            var countryGroupAverageTableQuery = (GroupBySqlHelper)this.historyQueryBuilder.BuildJoinApproveHistoryValueQuery(
                history,
                this.BuildSelectCountryGroupAverageTableQuery(history.Context),
                InputLevelJoinType.All,
                this.BuildCountryGroupAverageTableJoinInfos(costBlockMeta),
                historyValueId);

            var options = new QualityGateQueryOptions
            {
                InnerQualityGateQuery = this.historyQueryBuilder.BuildJoinApproveHistoryValueQuery(
                    history,
                    innerQualityGateQuery,
                    InputLevelJoinType.All,
                    historyValueId: historyValueId),

                CountryGroupAverageTableQuery = this.BuildGroupByCountryGroupAverageTableQuery(history.Context, countryGroupAverageTableQuery)
            };

            if (!historyValueId.HasValue)
            {
                options.ResultQualityGateTableAlias = "t";

                var inputLevelColumn = new ColumnInfo(history.Context.InputLevelId, options.ResultQualityGateTableAlias, history.Context.InputLevelId);
                var checkColumns = this.BuildQualityGateQueryCheckColumns(history.Context);
                var innerColumns = new BaseColumnInfo[]
                {
                    SqlFunctions.Min(PeriodCheckColumn, options.ResultQualityGateTableAlias, PeriodCheckColumn),
                    SqlFunctions.Min(CountryGroupCheckColumn, options.ResultQualityGateTableAlias, CountryGroupCheckColumn),
                    inputLevelColumn
                };
                var columns = this.BuildQualityGateQueryColumns(history.Context, checkColumns).Concat(innerColumns);

                options.ResultQuery =
                    Sql.Select(columns.Select(column => column.Alias).ToArray())
                       .FromQuery(
                            Sql.Select(innerColumns)
                               .From(options.ResultQualityGateTableAlias)
                               .GroupBy(
                                    inputLevelColumn,
                                    new ColumnInfo(PeriodCheckColumn, options.ResultQualityGateTableAlias),
                                    new ColumnInfo(CountryGroupCheckColumn, options.ResultQualityGateTableAlias)),
                            GroupedByInputLevelTable)
                       .Join(
                            new AliasSqlBuilder
                            {
                                Alias = ApproveHistoryValueTable,
                                Query = new BracketsSqlBuilder
                                {
                                    Query = this.historyQueryBuilder.BuildSelectJoinApproveHistoryValueQuery(history, inputLevelIdAlias: inputLevelColumn.Name)
                                                                    .ToSqlBuilder()
                                }
                            },
                            SqlOperators.Equals(
                                new ColumnInfo(inputLevelColumn.Name, ApproveHistoryValueTable), 
                                new ColumnInfo(inputLevelColumn.Name, GroupedByInputLevelTable)),
                            JoinType.Left);
            }

            return this.BuildQualityGateQuery(history.Context, options);
        }

        private string BuildAlias(string column, string table)
        {
            return $"{table}_{column}";
        }

        private SqlHelper BuildQualityGateQuery(HistoryContext historyContext, QualityGateQueryOptions options)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var checkColumns = this.BuildQualityGateQueryCheckColumns(historyContext);

            var columns = this.BuildQualityGateQueryColumns(historyContext, checkColumns);

            var innerColumns =
                costBlockMeta.DependencyFields.Select(field => new ColumnInfo(field.Name, InnerQualityGateTable))
                                              .Concat(checkColumns.Cast<BaseColumnInfo>())
                                              .ToList();

            innerColumns.Add(new ColumnInfo(MetaConstants.WgInputLevelName, InnerQualityGateTable));
            innerColumns.Add(new ColumnInfo(NewValueColumn, InnerQualityGateTable));
            innerColumns.Add(new ColumnInfo(historyContext.InputLevelId, InnerQualityGateTable));

            var falseValue = new RawSqlBuilder("0");
            var whereCondition = SqlOperators.Equals(new ColumnSqlBuilder(PeriodCheckColumn, ResultQualityGateTable), falseValue)
                                             .Or(SqlOperators.Equals(
                                                 new ColumnSqlBuilder(CountryGroupCheckColumn, ResultQualityGateTable),
                                                 falseValue));

            var wgField = costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName];

            var qualityQateResultQuery =
                Sql.Select(columns.ToArray())
                   .FromQuery(
                       Sql.Select(innerColumns.ToArray())
                           .FromQuery(options.InnerQualityGateQuery, InnerQualityGateTable),
                       ResultQualityGateTable)
                   .Join(costBlockMeta, wgField.Name, metaTableAlias: ResultQualityGateTable)
                   .Join(costBlockMeta.DependencyFields.Select(field => new JoinInfo(costBlockMeta, field.Name, metaTableAlias: ResultQualityGateTable)))
                   .Where(whereCondition)
                   .ToSqlBuilder();

            var withCountryGroupAverageTable = new WithQuery
            {
                Name = CountryGroupAverageTable,
                ColumnNames = this.BuildCountryGroupAverageColumns(historyContext).Select(column => column.Alias),
                Query = options.CountryGroupAverageTableQuery.ToSqlBuilder()
            };

            SqlHelper query;

            if (options.ResultQuery == null)
            {
                query = Sql.With(qualityQateResultQuery, withCountryGroupAverageTable);
            }
            else
            {
                query = Sql.With(
                    options.ResultQuery,
                    withCountryGroupAverageTable,
                    new WithQuery
                    {
                        Name = options.ResultQualityGateTableAlias,
                        ColumnNames = columns.Select(column => column.Alias).ToArray(),
                        Query = qualityQateResultQuery
                    });
            }


            return query;
        }

        private QueryColumnInfo[] BuildQualityGateQueryCheckColumns(HistoryContext historyContext)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var qualityGate = this.domainMeta.CostBlocks[historyContext.CostBlockId].QualityGate;
            var periodCheckColumn = BuildCheckResultColumn(
                InnerQualityGateTable,
                OldValueColumn,
                NewValueColumn,
                PeriodCheckColumn,
                qualityGate.PeriodCoeff,
                PeriodCoeffParam);

            var countryGroupCheckColumn = BuildCheckResultColumn(
                    InnerQualityGateTable,
                    CountryGroupAvgColumn,
                    NewValueColumn,
                    CountryGroupCheckColumn,
                    qualityGate.CountryGroupCoeff,
                    CountryGroupCoeffParam);

           return new [] { periodCheckColumn, countryGroupCheckColumn };
        }

        private List<ColumnInfo> BuildQualityGateQueryColumns(HistoryContext historyContext, IEnumerable<BaseColumnInfo> checkColumns)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            
            
            var wgField = costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName];
            var columns = new List<ColumnInfo>
            {
                new ColumnInfo(NewValueColumn, ResultQualityGateTable, this.BuildAlias(NewValueColumn, ResultQualityGateTable)),
                new ColumnInfo(MetaConstants.IdFieldKey, wgField.ReferenceMeta.Name, this.BuildAlias(MetaConstants.IdFieldKey, wgField.ReferenceMeta.Name)),
                new ColumnInfo(MetaConstants.NameFieldKey, wgField.ReferenceMeta.Name, this.BuildAlias(MetaConstants.NameFieldKey, wgField.ReferenceMeta.Name))
            };

            columns.AddRange(costBlockMeta.DependencyFields.SelectMany(field => new[]
            {
                new ColumnInfo(MetaConstants.IdFieldKey, field.ReferenceMeta.Name, this.BuildAlias(MetaConstants.IdFieldKey, wgField.ReferenceMeta.Name)),
                new ColumnInfo(MetaConstants.NameFieldKey, field.ReferenceMeta.Name, this.BuildAlias(MetaConstants.NameFieldKey, wgField.ReferenceMeta.Name))
            }));

            columns.AddRange(checkColumns.Select(column => new ColumnInfo(column.Alias, ResultQualityGateTable, this.BuildAlias(column.Alias, ResultQualityGateTable))));
            columns.Add(new ColumnInfo(historyContext.InputLevelId, ResultQualityGateTable, this.BuildAlias(historyContext.InputLevelId, ResultQualityGateTable)));

            return columns;
        }

        private BaseColumnInfo[] BuildCountryGroupAverageColumns(HistoryContext historyContext)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);

            return new BaseColumnInfo[]
            {
                new ColumnInfo(this.countryGroupIdColumnName, CountryTable, this.countryGroupIdColumnName),
                new ColumnInfo(MetaConstants.WgInputLevelName, costBlockMeta.Name, MetaConstants.WgInputLevelName),
                SqlFunctions.Average(historyContext.CostElementId, costBlockMeta.HistoryMeta.Name, CountryGroupAverageColumn)
            };
        }

        private SqlHelper BuildNewValuesQuery(IEnumerable<EditItem> items, string idColumnAlias)
        {
            var editItemColumns = items.Select((editItem, index) => new
            {
                Id = new QueryColumnInfo
                {
                    Alias = idColumnAlias,
                    Query = new ParameterSqlBuilder
                    {
                        ParamInfo = new CommandParameterInfo($"id_{index}", editItem.Id)
                    }
                },
                Value = new QueryColumnInfo
                {
                    Alias = NewValueColumn,
                    Query = new ParameterSqlBuilder
                    {
                        ParamInfo = new CommandParameterInfo($"value_{index}", editItem.Value)
                    }
                }
            });

            return Sql.Union(
                editItemColumns.Select(columns => Sql.Select(columns.Id, columns.Value).ToSqlBuilder()));
        }

        private IEnumerable<JoinInfo> BuildCountryGroupAverageTableJoinInfos(CostBlockEntityMeta costBlockMeta)
        {
            return new[]
            {
                new JoinInfo
                {
                    Meta = costBlockMeta,
                    ReferenceFieldName = MetaConstants.CountryInputLevelName,
                    JoinedTableAlias = CountryTable
                }
            };
        }

        private SelectJoinSqlHelper BuildSelectCountryGroupAverageTableQuery(HistoryContext historyContext)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);

            return Sql.Select(this.BuildCountryGroupAverageColumns(historyContext))
                      .From(costBlockMeta.HistoryMeta);
        }

        private ConditionHelper BuildWhereCountryGroupAverageTableQuery(CostBlockEntityMeta costBlockMeta, IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var whereCondition = SqlOperators.Equals(
                nameof(CostBlockHistory.State),
                "costBlockHistoryState",
                (int)CostBlockHistoryState.Approved,
                MetaConstants.CostBlockHistoryTableName);

            if (costBlockFilter != null && costBlockFilter.Count > 0)
            {
                whereCondition = whereCondition.And(costBlockFilter, costBlockMeta.Name);
            }

            return whereCondition;
        }

        private SqlHelper BuildGroupByCountryGroupAverageTableQuery(
            HistoryContext historyContext,
            GroupBySqlHelper query)
        {
            var columns = this.BuildCountryGroupAverageColumns(historyContext);

            return query.GroupBy(columns.OfType<ColumnInfo>().ToArray());
        }

        private SelectSqlHelper BuildSelectInnerQualityGateQuery(HistoryContext historyContext)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var coordinateColumns =
                costBlockMeta.InputLevelFields.Concat(costBlockMeta.DependencyFields)
                                              .Select(field => new ColumnInfo(field.Name, InnerQualityGateCostBlockTable));

            var approvedCostElement = costBlockMeta.GetApprovedCostElement(historyContext.CostElementId);
            var columns = new List<BaseColumnInfo>(coordinateColumns)
            {
                new ColumnInfo(NewValueColumn, NewValuesTable),
                new ColumnInfo(approvedCostElement.Name, InnerQualityGateCostBlockTable, OldValueColumn)
            };

            var wgColumn =
                columns.OfType<ColumnInfo>()
                       .First(column => column.Name == MetaConstants.WgInputLevelName);

            columns.Add(new QueryColumnInfo
            {
                Alias = CountryGroupAvgColumn,
                Query =
                    Sql.Select(CountryGroupAverageColumn)
                       .From(CountryGroupAverageTable)
                       .Where(ConditionHelper.And(
                           SqlOperators.Equals(
                               new ColumnInfo(this.countryGroupIdColumnName, CountryGroupAverageTable),
                               new ColumnInfo(this.countryGroupIdColumnName, InnerQualityGateCountryTable)),
                           SqlOperators.Equals(
                               wgColumn,
                               new ColumnInfo(MetaConstants.WgInputLevelName, CountryGroupAverageTable))))
                       .ToSqlBuilder()
            });

            return
                 Sql.Select(columns.ToArray());
        }

        private SelectJoinSqlHelper BuildJoinCountryInnerQualityGateQuery(CostBlockEntityMeta costBlockMeta, SelectJoinSqlHelper query)
        {
            return query.Join(costBlockMeta, MetaConstants.CountryInputLevelName, InnerQualityGateCountryTable, InnerQualityGateCostBlockTable);
        }

        private QueryColumnInfo BuildCheckResultColumn(
           string table,
           string oldValueColumnName,
           string newValueColumnName,
           string alias,
           double coeff,
           string coeffParamName)
        {
            var oldValueColumn = new ColumnInfo(oldValueColumnName, table);
            var newValueColumn = new ColumnInfo(newValueColumnName, table);

            var when = SqlOperators.Greater(
                new AbsSqlBuilder
                {
                    Query = SqlOperators.Subtract(newValueColumn, oldValueColumn).ToSqlBuilder()
                },
                new AbsSqlBuilder
                {
                    Query = SqlOperators.Multiply(oldValueColumnName, coeffParamName, coeff, table).ToSqlBuilder()
                });

            return new QueryColumnInfo
            {
                Alias = alias,
                Query = new CaseSqlBuilder
                {
                    Cases = new List<CaseItem>
                    {
                        new CaseItem
                        {
                            When = when.ToSqlBuilder(),
                            Then = new RawSqlBuilder("0")
                        }
                    },
                    Else = new RawSqlBuilder("1")
                }
            };
        }

        private class QualityGateQueryOptions
        {
            public SqlHelper InnerQualityGateQuery { get; set; }

            public SqlHelper CountryGroupAverageTableQuery { get; set; }

            public SqlHelper ResultQuery { get; set; }

            public string ResultQualityGateTableAlias { get; set; }
        }
    }
}
