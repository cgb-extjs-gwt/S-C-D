using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
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

        private const string InnerQualityGateCountryTable = "InnerQualityGateCountryTable";

        private const string CountryGroupCheckColumn = "CountryGroupCheck";

        private const string CountryGroupCoeffParam = "сountryGroupCoeff";

        private const string PeriodCheckColumn = "PeriodCheck";

        private const string PeriodCoeffParam = "periodCoeff";

        private const string CountryGroupAverageTable = "CountryGroupAverageTable";

        private const string CountryGroupAverageColumn = "CountryGroupAverageColumn";

        private const string CountryTableAlias = "CountryTable";

        private const string CostElementValuesTable = "CostElementValuesTable";

        private const string HistoryValueIdColumn = "HistoryValueIdColumn";

        private readonly string qualityGateCountryGroupColumnName;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ICostBlockValueHistoryQueryBuilder historyQueryBuilder;

        public QualityGateQueryBuilder(ICostBlockValueHistoryQueryBuilder historyQueryBuilder, DomainEnitiesMeta domainEnitiesMeta)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.historyQueryBuilder = historyQueryBuilder;

            var countryMeta = this.domainEnitiesMeta.GetCountryEntityMeta();

            this.qualityGateCountryGroupColumnName = countryMeta.QualityGateGroupField.Name;
        }

        public SqlHelper BuildQualityGateQuery(
            HistoryContext historyContext,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter,
            bool useCountryGroupCheck)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var options = new QualityGateQueryOptions
            {
                OnlyFailed = true,
                UseCountryGroupCheck = useCountryGroupCheck
            };

            var costElementValueColumns = this.BuildCostElementValueTableColumns(historyContext, options, NewValueColumn, costBlockMeta.Name, NewValuesTable);

            var costElementValueTableQuery =
                 Sql.Select(costElementValueColumns.ToArray())
                   .FromQuery(this.BuildNewValuesQuery(editItems, historyContext.InputLevelId), NewValuesTable)
                   .Join(
                       costBlockMeta,
                       SqlOperators.Equals(
                           new ColumnInfo(historyContext.InputLevelId, NewValuesTable),
                           new ColumnInfo(historyContext.InputLevelId, costBlockMeta.Name)));

            if (options.UseCountryGroupCheck)
            {
                costElementValueTableQuery = costElementValueTableQuery.Join(costBlockMeta, MetaConstants.CountryInputLevelName, CountryTableAlias);
            }

            options.CostElementValueTableQuery = costElementValueTableQuery.WhereNotDeleted(costBlockMeta, costBlockFilter, costBlockMeta.Name);

            return this.BuildQualityGateQuery(historyContext, options);
        }

        public SqlHelper BuildQualityGateQuery(
            CostBlockHistory history,
            bool useCountryGroupCheck, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var options = new QualityGateQueryOptions
            {
                OnlyFailed = true,
                UseCountryGroupCheck = useCountryGroupCheck
            };

            options.CostElementValueTableQuery = this.BuildCostElementValueTableQuery(history, options, null, costBlockFilter);

            return this.BuildQualityGateQuery(history.Context, options);
        }

        public SqlHelper BuildQulityGateApprovalQuery(
            CostBlockHistory history,
            bool useCountryGroupCheck,
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var options = new QualityGateQueryOptions
            {
                UseHistoryValueIdColumn = true,
                UseCountryGroupCheck = useCountryGroupCheck
            };
            options.CostElementValueTableQuery = this.BuildCostElementValueTableQuery(history, options, historyValueId, costBlockFilter);

            SqlHelper query;

            if (historyValueId.HasValue)
            {
                query = this.BuildQualityGateQuery(history.Context, options);
            }
            else
            {
                var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);

                options.MaxInputLevel = history.Context.InputLevelId;
                options.CustomCheckColumns =
                    this.BuildQualityGateQueryCheckColumns(costBlockMeta, options)
                        .Select(column => SqlFunctions.Min(column.Alias, ResultQualityGateTable, column.Alias));

                query = this.BuildQualityGateQuery(history.Context, options);

                var withSqlBuilder = (WithSqlBuilder)query.ToSqlBuilder();
                var groupBySqlHelper = new GroupBySqlHelper(withSqlBuilder.Query);
                var groupColumns = this.BuildQualityGateQueryColumns(costBlockMeta, options, history.Context);

                query = Sql.With(
                    groupBySqlHelper.GroupBy(groupColumns.ToArray()),
                    withSqlBuilder.WithQueries.ToArray());
            }

            return query;
        }

        private SqlHelper BuildCostElementValueTableQuery(
            CostBlockHistory history, 
            QualityGateQueryOptions options, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var costElementValueTableColumns = this.BuildCostElementValueTableColumns(
                history.Context, 
                options, 
                history.Context.CostElementId, 
                costBlockMeta.Name,
                costBlockMeta.HistoryMeta.Name);

            var costElementValueTableQuery =
                Sql.SelectDistinct(costElementValueTableColumns.ToArray())
                   .From(costBlockMeta.HistoryMeta);

            JoinInfo[] joinInfos = null;

            if (options.UseCountryGroupCheck)
            {
                joinInfos = new[]
                {
                     new JoinInfo
                     {
                        Meta = costBlockMeta,
                        ReferenceFieldName = MetaConstants.CountryInputLevelName,
                        JoinedTableAlias = CountryTableAlias
                     }
                };
            }

            return this.historyQueryBuilder.BuildJoinApproveHistoryValueQuery(
                history,
                costElementValueTableQuery,
                InputLevelJoinType.All,
                joinInfos,
                historyValueId,
                costBlockFilter);
        }

        private string BuildAlias(string item1, string item2)
        {
            return $"{item1}_{item2}";
        }

        private SqlHelper BuildQualityGateQuery(HistoryContext historyContext, QualityGateQueryOptions options, IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);

            var checkColumns = this.BuildQualityGateQueryCheckColumns(costBlockMeta, options);
            var domainCoordinateFields = costBlockMeta.GetDomainCoordinateFields(historyContext.CostElementId);
            var resultQualityGateTableColumns = 
                domainCoordinateFields.Select(field => new ColumnInfo(field.Name, InnerQualityGateTable))
                                      .Concat(checkColumns.OfType<BaseColumnInfo>())
                                      .Concat(this.BuildValueColumns(InnerQualityGateTable, options))
                                      .ToList();

            if (options.UseHistoryValueIdColumn)
            {
                resultQualityGateTableColumns.Add(new ColumnInfo(HistoryValueIdColumn, InnerQualityGateTable));
            }

            var columns = this.BuildQualityGateQueryColumns(costBlockMeta, options, historyContext).OfType<BaseColumnInfo>();
            if (options.CustomCheckColumns == null)
            {
                columns = columns.Concat(checkColumns.Select(
                    column => new ColumnInfo(column.Alias, ResultQualityGateTable, this.BuildAlias(ResultQualityGateTable, column.Alias))));
            }
            else
            {
                columns = columns.Concat(options.CustomCheckColumns);
            }

            SqlHelper qualityQateResultQuery =
                Sql.SelectDistinct(columns.ToArray())
                   .FromQuery(
                       Sql.Select(resultQualityGateTableColumns.ToArray())
                          .FromQuery(this.BuildInnerQualityGateQuery(historyContext, options), InnerQualityGateTable),
                       ResultQualityGateTable)
                   .Join(domainCoordinateFields.Select(field => new JoinInfo(costBlockMeta, field.Name, metaTableAlias: ResultQualityGateTable)));

            if (options.OnlyFailed)
            {
                var falseValue = new RawSqlBuilder("0");
                var whereCondition =
                    SqlOperators.Equals(new ColumnSqlBuilder(PeriodCheckColumn, ResultQualityGateTable), falseValue);

                if (options.UseCountryGroupCheck)
                {
                    whereCondition = whereCondition.Or(
                        SqlOperators.Equals(
                            new ColumnSqlBuilder(CountryGroupCheckColumn, ResultQualityGateTable),
                            falseValue));
                }

                qualityQateResultQuery = ((SelectJoinSqlHelper)qualityQateResultQuery).Where(whereCondition);
            }

            var costElementValuesColumns = this.BuildCostElementValueTableColumns(historyContext, options);
            var withCostElementValuesTable = new WithQuery
            {
                Name = CostElementValuesTable,
                ColumnNames = costElementValuesColumns.Select(column => column.Alias ?? column.Name),
                Query = options.CostElementValueTableQuery.ToSqlBuilder()
            };

            var wihtQueries = new List<WithQuery>
            {
                withCostElementValuesTable
            };

            if (options.UseCountryGroupCheck)
            {
                var withCountryGroupAverageTable = new WithQuery
                {
                    Name = CountryGroupAverageTable,
                    ColumnNames = this.BuildCountryGroupAverageColumns(historyContext).Select(column => column.Alias),
                    Query = this.BuildCountryGroupAverageTable(historyContext).ToSqlBuilder()
                };

                wihtQueries.Add(withCountryGroupAverageTable);
            }

            return Sql.With(qualityQateResultQuery, wihtQueries.ToArray());
        }

        private List<QueryColumnInfo> BuildQualityGateQueryCheckColumns(CostBlockEntityMeta costBlockMeta, QualityGateQueryOptions options)
        {
            var qualityGate = costBlockMeta.DomainMeta.QualityGate;
            var periodCheckColumn = BuildCheckResultColumn(
                InnerQualityGateTable,
                OldValueColumn,
                NewValueColumn,
                PeriodCheckColumn,
                qualityGate.PeriodCoeff,
                PeriodCoeffParam);

            var checkColumns = new List<QueryColumnInfo> { periodCheckColumn };

            if (options.UseCountryGroupCheck)
            {
                var countryGroupCheckColumn = BuildCheckResultColumn(
                        InnerQualityGateTable,
                        CountryGroupAvgColumn,
                        NewValueColumn,
                        CountryGroupCheckColumn,
                        qualityGate.CountryGroupCoeff,
                        CountryGroupCoeffParam);

                checkColumns.Add(countryGroupCheckColumn);
            }

            return checkColumns;
        }

        private IEnumerable<ColumnInfo> BuildValueColumns(string table, QualityGateQueryOptions options)
        {
            yield return new ColumnInfo(NewValueColumn, table);
            yield return new ColumnInfo(OldValueColumn, table);

            if (options.UseCountryGroupCheck)
            {
                yield return new ColumnInfo(CountryGroupAvgColumn, table);
            }
        }

        private List<ColumnInfo> BuildQualityGateQueryColumns(CostBlockEntityMeta costBlockMeta, QualityGateQueryOptions options, HistoryContext historyContext)
        {
            var columns = this.BuildValueColumns(ResultQualityGateTable, options).ToList();

            columns.ForEach(column => column.Alias = this.BuildAlias(column.TableName, column.Name));

            if (options.UseHistoryValueIdColumn)
            {
                columns.Add(new ColumnInfo(HistoryValueIdColumn, ResultQualityGateTable));
            }

            var costElement = costBlockMeta.DomainMeta.CostElements[historyContext.CostElementId];
            var inputLevels = options.MaxInputLevel == null
                ? costElement.InputLevels
                : costElement.FilterInputLevels(options.MaxInputLevel);

            var fields = inputLevels.Select(inputLevel => costBlockMeta.InputLevelFields[inputLevel.Id]).ToList();
            var dependencyField = costBlockMeta.GetDomainDependencyField(historyContext.CostElementId);
            if (dependencyField != null)
            {
                fields.Add(dependencyField);
            }

            columns.AddRange(fields.SelectMany(field => new[]
            {
                new ColumnInfo(MetaConstants.IdFieldKey, field.ReferenceMeta.Name, this.BuildAlias(field.ReferenceMeta.Name, MetaConstants.IdFieldKey)),
                new ColumnInfo(MetaConstants.NameFieldKey, field.ReferenceMeta.Name, this.BuildAlias(field.ReferenceMeta.Name, MetaConstants.NameFieldKey))
            }));

            return columns;
        }

        private BaseColumnInfo[] BuildCountryGroupAverageColumns(HistoryContext historyContext)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var approvedCostElement = costBlockMeta.GetApprovedCostElement(historyContext.CostElementId);

            return new BaseColumnInfo[]
            {
                new ColumnInfo(this.qualityGateCountryGroupColumnName, CostElementValuesTable, this.qualityGateCountryGroupColumnName),
                new QueryColumnInfo
                {
                    Alias = CountryGroupAverageColumn,
                    Query =
                        Sql.Select(SqlFunctions.Average(approvedCostElement.Name, costBlockMeta.Name, CountryGroupAverageColumn))
                           .From(costBlockMeta)
                           .Join(costBlockMeta, MetaConstants.CountryInputLevelName)
                           .Where(SqlOperators.Equals(
                               new ColumnInfo(this.qualityGateCountryGroupColumnName, CostElementValuesTable),
                               new ColumnInfo(this.qualityGateCountryGroupColumnName, MetaConstants.CountryInputLevelName)))
                           .ToSqlBuilder()
                }
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

        private SqlHelper BuildCountryGroupAverageTable(HistoryContext historyContext)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var columns = this.BuildCountryGroupAverageColumns(historyContext);

            var qualityGroupCountryGroupColumn = 
                    columns.OfType<ColumnInfo>().First(column => column.Name == this.qualityGateCountryGroupColumnName);

            return 
                Sql.Select(columns)
                   .From(CostElementValuesTable)
                   .GroupBy(qualityGroupCountryGroupColumn);
        }

        private List<ColumnInfo> BuildCostElementValueTableColumns(
            HistoryContext historyContext,
            QualityGateQueryOptions options,
            string newValueColumn = NewValueColumn, 
            string table = null, 
            string newValueColumnTable = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var columns = 
                costBlockMeta.GetDomainCoordinateFields(historyContext.CostElementId)
                             .Select(field => new ColumnInfo(field.Name, table))
                             .ToList();

            columns.Add(new ColumnInfo(newValueColumn, newValueColumnTable ?? table, NewValueColumn));

            var approvedCostElement = costBlockMeta.GetApprovedCostElement(historyContext.CostElementId);

            columns.Add(new ColumnInfo(approvedCostElement.Name, table, OldValueColumn));

            if (options.UseCountryGroupCheck)
            {
                columns.Add(new ColumnInfo(this.qualityGateCountryGroupColumnName, CountryTableAlias));
            }

            if (options.UseHistoryValueIdColumn)
            {
                columns.Add(new ColumnInfo(MetaConstants.IdFieldKey, costBlockMeta.HistoryMeta.Name, HistoryValueIdColumn));
            }

            return columns;
        }

        private SqlHelper BuildInnerQualityGateQuery(HistoryContext historyContext, QualityGateQueryOptions options)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var coordinateColumns = 
                costBlockMeta.GetDomainCoordinateFields(historyContext.CostElementId)
                             .Select(field => new ColumnInfo(field.Name, CostElementValuesTable));

            var columns = new List<BaseColumnInfo>(coordinateColumns)
            {
                new ColumnInfo(NewValueColumn, CostElementValuesTable),
                new ColumnInfo(OldValueColumn, CostElementValuesTable)
            };

            if (options.UseHistoryValueIdColumn)
            {
                columns.Add(new ColumnInfo(HistoryValueIdColumn, CostElementValuesTable));
            }

            if (options.UseCountryGroupCheck)
            {
                columns.Add(new QueryColumnInfo
                {
                    Alias = CountryGroupAvgColumn,
                    Query =
                        Sql.Select(CountryGroupAverageColumn)
                           .From(CountryGroupAverageTable)
                           .Where(SqlOperators.Equals(
                                new ColumnInfo(this.qualityGateCountryGroupColumnName, CountryGroupAverageTable),
                                new ColumnInfo(this.qualityGateCountryGroupColumnName, InnerQualityGateCountryTable)))
                           .ToSqlBuilder()
                });
            }

            var query = Sql.Select(columns.ToArray()).From(CostElementValuesTable);

            if (options.UseCountryGroupCheck)
            {
                query = query.Join(costBlockMeta, MetaConstants.CountryInputLevelName, InnerQualityGateCountryTable, CostElementValuesTable);
            }

            return query;
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
            public IEnumerable<BaseColumnInfo> CustomCheckColumns { get; set; }

            public SqlHelper CostElementValueTableQuery { get; set; }

            public bool OnlyFailed { get; set; }

            public bool UseHistoryValueIdColumn { get; set; }

            public string MaxInputLevel { get; set; }

            public bool UseCountryGroupCheck { get; set; }
        }
    }
}
