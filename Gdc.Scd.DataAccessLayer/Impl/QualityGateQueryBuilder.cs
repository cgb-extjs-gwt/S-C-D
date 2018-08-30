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
            var newValuesQuery = this.BuildNewValuesQuery(editItems, historyContext.InputLevelId);

            return this.BuildQualityGateQuery(historyContext, costBlockFilter, newValuesQuery);
        }

        public SqlHelper BuildQulityGateHistoryQuery(
            CostBlockHistory history, 
            long historyValueId,
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var newValuesQuery =
                this.historyQueryBuilder.BuildSelectJoinHistoryValueQuery(
                    history,
                    MetaConstants.WgInputLevelName,
                    InputLevelJoinType.HistoryContext,
                    historyValueId,
                    NewValueColumn);

            return this.BuildQualityGateQuery(history.Context, costBlockFilter, newValuesQuery);
        }

        private SqlHelper BuildQualityGateQuery(
            HistoryContext historyContext,
            IDictionary<string, IEnumerable<object>> costBlockFilter,
            SqlHelper newValuesQuery)
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

            var checkColumns = new List<BaseColumnInfo> { periodCheckColumn, countryGroupCheckColumn };

            var falseValue = new RawSqlBuilder("0");
            var whereCondition = SqlOperators.Equals(new ColumnSqlBuilder(PeriodCheckColumn, ResultQualityGateTable), falseValue)
                                             .Or(SqlOperators.Equals(
                                                 new ColumnSqlBuilder(CountryGroupCheckColumn, ResultQualityGateTable),
                                                 falseValue));

            var wgField = costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName];
            var columns = new List<ColumnInfo>
            {
                new ColumnInfo(NewValueColumn, ResultQualityGateTable),
                new ColumnInfo(MetaConstants.IdFieldKey, wgField.ReferenceMeta.Name),
                new ColumnInfo(MetaConstants.NameFieldKey, wgField.ReferenceMeta.Name)
            };

            columns.AddRange(costBlockMeta.DependencyFields.SelectMany(field => new[]
            {
                new ColumnInfo(MetaConstants.IdFieldKey, field.ReferenceMeta.Name),
                new ColumnInfo(MetaConstants.NameFieldKey, field.ReferenceMeta.Name)
            }));

            columns.AddRange(checkColumns.Select(column => new ColumnInfo(column.Alias, ResultQualityGateTable)));

            var innerColumns =
                costBlockMeta.DependencyFields.Select(field => new ColumnInfo(field.Name, InnerQualityGateTable))
                                              .Concat(checkColumns)
                                              .ToList();

            innerColumns.Add(new ColumnInfo(MetaConstants.WgInputLevelName, InnerQualityGateTable));
            innerColumns.Add(new ColumnInfo(NewValueColumn, InnerQualityGateTable));

            var query =
                Sql.Select(columns.ToArray())
                   .FromQuery(
                        Sql.Select(innerColumns.ToArray())
                           .FromQuery(
                                this.BuildInnerQualityGateQuery(historyContext, costBlockFilter, newValuesQuery),
                                InnerQualityGateTable),
                        ResultQualityGateTable)
                   .Join(costBlockMeta, wgField.Name, metaTableAlias: ResultQualityGateTable)
                   .Join(costBlockMeta.DependencyFields.Select(field => new JoinInfo(costBlockMeta, field.Name, metaTableAlias: ResultQualityGateTable)));

            return query.Where(whereCondition);
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

        private SqlHelper BuildCountryGroupQualityGateQuery(
            ColumnInfo wgColumn,
            HistoryContext historyContext,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            const string QualityGateCountryTable = "QualityGateCountryTable";

            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var query =
                Sql.Select(SqlFunctions.Average(historyContext.CostElementId, costBlockMeta.HistoryMeta.Name))
                   .From(costBlockMeta.HistoryMeta);

            var options = new JoinHistoryValueQueryOptions
            {
                InputLevelJoinType = InputLevelJoinType.All
            };

            query =
                this.historyQueryBuilder.BuildJoinHistoryValueQuery(historyContext, query, options)
                                        .Join(costBlockMeta, MetaConstants.CountryInputLevelName, QualityGateCountryTable);

            var filter = new Dictionary<string, IEnumerable<object>>(costBlockFilter);
            filter.Remove(MetaConstants.CountryInputLevelName);

            var countryGroupIdColumnName = $"{nameof(CountryGroup)}{nameof(CountryGroup.Id)}";

            var whereCondition =
                ConditionHelper.AndStatic(filter, costBlockMeta.Name)
                               .And(SqlOperators.Equals(
                                   new ColumnInfo(countryGroupIdColumnName, QualityGateCountryTable),
                                   new ColumnInfo(countryGroupIdColumnName, InnerQualityGateCountryTable)))
                               .And(SqlOperators.Equals(
                                   wgColumn,
                                   new ColumnInfo(MetaConstants.WgInputLevelName, costBlockMeta.Name)))
                               .And(SqlOperators.Equals(
                                   nameof(CostBlockHistory.State), 
                                   "costBlockHistoryState", 
                                   (int)CostBlockHistoryState.Approved, 
                                   MetaConstants.CostBlockHistoryTableName));

            return query.Where(whereCondition);
        }

        private SqlHelper BuildInnerQualityGateQuery(
            HistoryContext historyContext,
            IDictionary<string, IEnumerable<object>> costBlockFilter,
            SqlHelper newValueQuery)
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
                Query = this.BuildCountryGroupQualityGateQuery(wgColumn, historyContext, costBlockFilter).ToSqlBuilder()
            });

            var costBlockJoinCondition =
                SqlOperators.Equals(
                    new ColumnInfo(historyContext.InputLevelId, NewValuesTable),
                    new ColumnInfo(historyContext.InputLevelId, InnerQualityGateCostBlockTable))
                            .And(SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, InnerQualityGateCostBlockTable));

            var query =
                Sql.Select(columns.ToArray())
                   .FromQuery(newValueQuery, NewValuesTable)
                   .Join(costBlockMeta, costBlockJoinCondition, aliasMetaTable: InnerQualityGateCostBlockTable)
                   .Join(costBlockMeta, MetaConstants.CountryInputLevelName, InnerQualityGateCountryTable, InnerQualityGateCostBlockTable);

            return query.Where(costBlockFilter, InnerQualityGateCostBlockTable);
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
    }
}
