using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class QualityGateRepository : IQualityGateRepository
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

        private readonly ICostBlockValueHistoryQueryBuilder historyQueryBuilder;

        private readonly IRepositorySet repositorySet;

        private readonly DomainMeta domainMeta;

        public QualityGateRepository(ICostBlockValueHistoryQueryBuilder historyQueryBuilder, IRepositorySet repositorySet, DomainMeta domainMeta)
        {
            this.historyQueryBuilder = historyQueryBuilder;
            this.repositorySet = repositorySet;
            this.domainMeta = domainMeta;
        }

        public async Task<IEnumerable<QualityGateError>> Check(
            HistoryContext historyContext,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            var costBlockMeta = historyQueryBuilder.GetCostBlockEntityMeta(historyContext);
            var qualityGateQuery = this.BuildResutlQualityGateQuery(historyContext, costBlockMeta, editItems, costBlockFilter);

            return await this.repositorySet.ReadBySql(qualityGateQuery, reader => 
            {
                var index = 0;
                var result = new QualityGateError
                {
                    WarrantyGroup = new NamedId
                    {
                        Id = reader.GetInt64(index++),
                        Name = reader.GetString(index++),
                    },
                    Dependencies = new Dictionary<string, NamedId>()
                };

                foreach (var field in costBlockMeta.DependencyFields)
                {
                    result.Dependencies.Add(field.Name, new NamedId
                    {
                        Id = reader.GetInt64(index++),
                        Name = reader.GetString(index++)
                    });
                }

                result.IsPeriodError = reader.GetInt32(index++) == 0;
                result.IsRegionError = reader.GetInt32(index++) == 0;

                return result;
            });
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
            CostBlockEntityMeta costBlockMeta, 
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            const string QualityGateCountryTable = "QualityGateCountryTable";

            var averageColumn = new QueryColumnInfo(new AverageSqlBuilder
            {
                Query = new ColumnSqlBuilder(historyContext.CostElementId, costBlockMeta.HistoryMeta.Name)
            });

            var query =
                Sql.Select(averageColumn)
                   .From(costBlockMeta.HistoryMeta);

            query =
                historyQueryBuilder.BuildJoinHistoryValueQuery(historyContext, query)
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
                                   new ColumnInfo(MetaConstants.WgInputLevelName, costBlockMeta.Name)));

            return query.Where(whereCondition);
        }

        private SqlHelper BuildInnerQualityGateQuery(
            HistoryContext historyContext, 
            CostBlockEntityMeta costBlockMeta,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            var coordinateColumns =
                costBlockMeta.InputLevelFields.Concat(costBlockMeta.DependencyFields)
                                              .Select(field => new ColumnInfo(field.Name, InnerQualityGateCostBlockTable));

            var columns = new List<BaseColumnInfo>(coordinateColumns)
                {
                    new ColumnInfo(NewValueColumn, NewValuesTable),
                    new ColumnInfo(historyContext.CostElementId, InnerQualityGateCostBlockTable, OldValueColumn),
                };

            var wgColumn =
                columns.OfType<ColumnInfo>()
                        .First(column => column.Name == MetaConstants.WgInputLevelName);

            columns.Add(new QueryColumnInfo
            {
                Alias = CountryGroupAvgColumn,
                Query = this.BuildCountryGroupQualityGateQuery(wgColumn, historyContext, costBlockMeta, costBlockFilter).ToSqlBuilder()
            });

            var costBlockJoinCondition =
                SqlOperators.Equals(
                    new ColumnInfo(historyContext.InputLevelId, NewValuesTable),
                    new ColumnInfo(historyContext.InputLevelId, InnerQualityGateCostBlockTable))
                            .And(SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, InnerQualityGateCostBlockTable));

            var query =
                Sql.Select(columns.ToArray())
                   .FromQuery(BuildNewValuesQuery(editItems, historyContext.InputLevelId), NewValuesTable)
                   .Join(costBlockMeta, costBlockJoinCondition, aliasMetaTable: InnerQualityGateCostBlockTable)
                   .Join(costBlockMeta, MetaConstants.CountryInputLevelName, InnerQualityGateCountryTable, InnerQualityGateCostBlockTable);

            return query.Where(costBlockFilter, InnerQualityGateCostBlockTable);
        }

        SqlHelper BuildResutlQualityGateQuery(HistoryContext historyContext,
            CostBlockEntityMeta costBlockMeta,
            IEnumerable<EditItem> editItems,
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
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

            var query =
                Sql.Select(columns.ToArray())
                   .FromQuery(
                        Sql.Select(innerColumns.ToArray())
                           .FromQuery(
                                this.BuildInnerQualityGateQuery(historyContext, costBlockMeta, editItems, costBlockFilter), 
                                InnerQualityGateTable),
                        ResultQualityGateTable)
                   .Join(costBlockMeta, wgField.Name, metaTableAlias: ResultQualityGateTable)
                   .Join(costBlockMeta.DependencyFields.Select(field => new JoinInfo(costBlockMeta, field.Name, metaTableAlias: ResultQualityGateTable)));

            return query.Where(whereCondition);
        }

        QueryColumnInfo BuildCheckResultColumn(
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
