using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryRepository : ICostBlockValueHistoryRepository
    {
        private const string ValueColumnName = "value";

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly DomainMeta domainMeta;

        private readonly IRepositorySet repositorySet;

        public CostBlockValueHistoryRepository(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta, IRepositorySet repositorySet)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.repositorySet = repositorySet;
            this.domainMeta = domainMeta;
        }

        public async Task Save(CostBlockHistory history, IEnumerable<EditItem> editItems, IDictionary<string, long[]> relatedItems)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(history.Context);
            var values = editItems.Select(editItem => new object[] { history.Id, editItem.Id, editItem.Value });

            var insertValueQuery =
                Sql.Insert(
                    costBlockMeta.HistoryMeta, 
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, 
                    history.Context.InputLevelId, 
                    history.Context.CostElementId)
                   .Values(values, "history");

            var insertRelatedItemsQueries = new List<SqlHelper>();

            foreach (var relatedMeta in costBlockMeta.InputLevelFields.Concat(costBlockMeta.DependencyFields))
            {
                var historyRelatedMeta = costBlockMeta.HistoryMeta.GetRelatedMetaByName(relatedMeta.Name);
                var insertQuery = Sql.Insert(
                    historyRelatedMeta,
                    historyRelatedMeta.CostBlockHistoryField.Name,
                    historyRelatedMeta.RelatedItemField.Name);

                if (relatedItems.TryGetValue(relatedMeta.Name, out var relatedItemIds) && relatedItemIds.Length > 0)
                {
                    var relatedItemsInsertValues = relatedItemIds.Select(relatedItemId => new object[] { history.Id, relatedItemId });

                    insertRelatedItemsQueries.Add(insertQuery.Values(relatedItemsInsertValues, relatedMeta.Name));
                }
                else
                {
                    insertRelatedItemsQueries.Add(insertQuery.Values(relatedMeta.Name, history.Id, null));
                }
            }

            var queries = new List<SqlHelper>(insertRelatedItemsQueries)
            {
                insertValueQuery
            };

            await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetByCostBlockHistory(CostBlockHistory history)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(history.Context);
            var inputLevelField = costBlockMeta.InputLevelFields[history.Context.InputLevelId];
            var inputLevelMeta = (NamedEntityMeta)inputLevelField.ReferenceMeta;

            var inputLevelIdColumn = new ColumnInfo(inputLevelMeta.IdField.Name, costBlockMeta.HistoryMeta.Name, "InputLevelId");
            var inputLevelAlias = this.GetAlias(costBlockMeta.HistoryMeta);
            var inputLevelNameColumn = new ColumnInfo(inputLevelMeta.NameField.Name, inputLevelAlias, "InputLevelName");

            var selectColumns = new List<ColumnInfo> { inputLevelIdColumn, inputLevelNameColumn };
            selectColumns.AddRange(this.GetDependencyColumns(costBlockMeta));

            var selectQuery = this.BuildSelectHistoryValueQuery(history.Context, selectColumns);
            var joinInfos = this.GetDependencyJoinInfos(costBlockMeta).ToList();

            joinInfos.Add(new JoinInfo
            {
                Meta = costBlockMeta.HistoryMeta,
                ReferenceFieldName = history.Context.InputLevelId,
                JoinedTableAlias = inputLevelAlias
            });

            var query = this.BuildJoinApproveHistoryValueQuery(history, selectQuery, joinInfos);

            return await this.repositorySet.ReadBySql(query, this.CostBlockValueHistoryMap);
        }

        public async Task<IEnumerable<CostBlockHistoryValueDto>> GetCostBlockHistoryValueDto(
            HistoryContext historyContext, 
            IDictionary<string, IEnumerable<object>> filter,
            QueryInfo queryInfo = null)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);

            var historyEditUserIdColumnName = $"{nameof(CostBlockHistory.EditUser)}{nameof(User.Id)}";
            var historyEditUserIdColumnAlias = this.ToLowerFirstLetter(nameof(CostBlockHistoryValueDto.EditUserId));
            var histroryEditUserIdColumn = new ColumnInfo(
                historyEditUserIdColumnName, 
                costBlockMeta.HistoryMeta.CostBlockHistoryField.ReferenceMeta.Name, 
                historyEditUserIdColumnAlias);

            var editDateColumnAlias = this.ToLowerFirstLetter(nameof(CostBlockHistoryValueDto.EditDate));
            var editDateColumn = new ColumnInfo(
                nameof(CostBlockHistory.EditDate), 
                costBlockMeta.HistoryMeta.CostBlockHistoryField.ReferenceMeta.Name, 
                editDateColumnAlias);

            var userNameColumnAlias = this.ToLowerFirstLetter(nameof(CostBlockHistoryValueDto.EditUserName));
            var userNameColumn = new ColumnInfo(nameof(User.Name), nameof(User), userNameColumnAlias);

            var selectColumns = new List<ColumnInfo>
            {
                editDateColumn,
                histroryEditUserIdColumn,
                userNameColumn
            };
            
            var selectQuery = this.BuildSelectHistoryValueQuery(historyContext, selectColumns);

            if (queryInfo == null)
            {
                queryInfo = new QueryInfo();
            }

            if (queryInfo.Sort == null)
            {
                queryInfo.Sort = new SortInfo
                {
                    Direction = SortDirection.Desc,
                    Property = editDateColumnAlias
                };
            }

            var costBlockJoinCondition = this.GetHistoryInputLevelJoinCondition(historyContext);
            var costElement = this.domainMeta.CostBlocks[historyContext.CostBlockId].CostElements[historyContext.CostElementId];

            foreach (var inputLevel in costElement.InputLevels)
            {
                if (inputLevel.Id == historyContext.InputLevelId)
                {
                    break;
                }

                costBlockJoinCondition = costBlockJoinCondition.Or(
                    SqlOperators.Equals(
                        new ColumnInfo(inputLevel.Name, costBlockMeta.HistoryMeta.Name),
                        new ColumnInfo(inputLevel.Name, costBlockMeta.Name)));
            }

            var whereCondition =
                ConditionHelper.AndStatic(filter, costBlockMeta.Name)
                               .And(SqlOperators.IsNotNull(costElement.Id, costBlockMeta.HistoryMeta.Name));

            var userIdColumn = new ColumnInfo(nameof(User.Id), nameof(User));
            var options = new JoinHistoryValueQueryOptions
            {
                IsUseRegionCondition = true,
                CostBlockJoinAdditionalCondition = costBlockJoinCondition.ToSqlBuilder()
            };

            var query =
                this.BuildJoinHistoryValueQuery(historyContext, selectQuery, options)
                    .Join(nameof(User), SqlOperators.Equals(histroryEditUserIdColumn, userIdColumn))
                    .Where(whereCondition)
                    .ByQueryInfo(queryInfo);

            return await this.repositorySet.ReadBySql(query, reader => new CostBlockHistoryValueDto
            {
                Value = reader.GetValue(0),
                EditDate = reader.GetDateTime(1),
                EditUserId = reader.GetInt64(2),
                EditUserName = reader.GetString(3)
            });
        }

        public async Task<int> Approve(CostBlockHistory history)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(history.Context);
            var costElementField = costBlockMeta.CostElementsFields[history.Context.CostElementId];
            var historyValueColumn = new ColumnSqlBuilder
            {
                Table = costBlockMeta.HistoryMeta.Name,
                Name = costElementField.Name
            };

            var costElementColumn = new QueryUpdateColumnInfo(costElementField.Name, historyValueColumn, costBlockMeta.Name);
            var costElementApprovedField = costBlockMeta.CostElementsApprovedFields[costElementField];
            var costElementApprovedColumn = new QueryUpdateColumnInfo(costElementApprovedField.Name, historyValueColumn, costBlockMeta.Name);

            var updateQuery =
                Sql.Update(costBlockMeta, costElementColumn, costElementApprovedColumn)
                   .From(costBlockMeta.HistoryMeta);

            var query = this.BuildJoinApproveHistoryValueQuery(history, updateQuery);

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        public async Task<IEnumerable<QualityGateError>> QualityGateCheck(
            HistoryContext historyContext, 
            IEnumerable<EditItem> editItems, 
            IDictionary<string, IEnumerable<object>> costBlockFilter)
        {
            const string NewValueColumn = "NewValue";
            const string OldValueColumn = "OldValue";
            const string NewValuesTable = "NewValues";
            const string CountryGroupAvgColumn = "CountryGroupAvg";
            const string InnerQualityGateTable = "InnerQualityGateTable";
            const string ResultQualityGateTable = "ResultQualityGateTable";
            const string InnerQualityGateCostBlockTable = "InnerQualityGateCostBlockTable";
            const string InnerQualityGateCountryTable = "InnerQualityGateCountryTable";
            const string CountryGroupCheckColumn = "CountryGroupCheck";
            const string CountryGroupCoeffParam = "сountryGroupCoeff";
            const string PeriodCheckColumn = "PeriodCheck";
            const string PeriodCoeffParam = "periodCoeff";

            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);
            var qualityGateQuery = BuildResutlQualityGateQuery();

            return await this.repositorySet.ReadBySql(qualityGateQuery, ReaderMap);

            SqlHelper BuildNewValuesQuery(IEnumerable<EditItem> items, string idColumnAlias)
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

            SqlHelper BuildCountryGroupQualityGateQuery(ColumnInfo wgColumn)
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
                    this.BuildJoinHistoryValueQuery(historyContext, query)
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

            SqlHelper BuildInnerQualityGateQuery()
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
                    Query = BuildCountryGroupQualityGateQuery(wgColumn).ToSqlBuilder()
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

            SqlHelper BuildResutlQualityGateQuery()
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
                            Sql.Select(innerColumns.ToArray()).FromQuery(BuildInnerQualityGateQuery(), InnerQualityGateTable),
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

            QualityGateError ReaderMap(IDataReader reader)
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
            }
        }

        private SelectJoinSqlHelper BuildSelectHistoryValueQuery(HistoryContext historyContext, IEnumerable<BaseColumnInfo> addingSelectColumns)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);
            var inputLevelAlias = this.GetAlias(costBlockMeta.HistoryMeta);

            ColumnInfo costElementColumn;

            var costElementField = costBlockMeta.CostElementsFields[historyContext.CostElementId];
            var referenceCostElementField = costElementField as ReferenceFieldMeta;
            if (referenceCostElementField == null)
            {
                costElementColumn = new ColumnInfo(costElementField.Name, costBlockMeta.HistoryMeta.Name, ValueColumnName);
            }
            else
            {
                costElementColumn = new ColumnInfo(referenceCostElementField.ReferenceFaceField, referenceCostElementField.ReferenceMeta.Name, ValueColumnName);
            }

            var selectQuery =
                Sql.SelectDistinct(new[] { costElementColumn }.Concat(addingSelectColumns).ToArray())
                   .From(costBlockMeta.HistoryMeta);

            if (referenceCostElementField != null)
            {
                selectQuery = selectQuery.Join(costBlockMeta.HistoryMeta, referenceCostElementField.Name);
            }

            return selectQuery;
        }

        private TQuery BuildJoinHistoryValueQuery<TQuery>(HistoryContext historyContext, TQuery query, JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);

            var createdDateCondition =
                SqlOperators.LessOrEqual(
                    new ColumnInfo(costBlockMeta.CreatedDateField.Name, costBlockMeta.Name),
                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName));

            var deletedDateCondition = ConditionHelper.OrBrackets(
                SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                SqlOperators.GreaterOrEqual(new ColumnInfo(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName)));

            var costBlockJoinCondition = ConditionHelper.And(createdDateCondition, deletedDateCondition);

            if (options != null)
            {
                if (historyContext.RegionInputId.HasValue)
                {
                    var region = this.domainMeta.CostBlocks[historyContext.CostBlockId].CostElements[historyContext.CostElementId].RegionInput;
                    var historyRegionIdName = $"{nameof(CostBlockHistory.Context)}_{nameof(HistoryContext.RegionInputId)}";
                    var regionCondition = SqlOperators.Equals(
                            new ColumnInfo(historyRegionIdName, nameof(CostBlockHistory)),
                            new ColumnInfo(region.Id, costBlockMeta.Name));

                    costBlockJoinCondition = costBlockJoinCondition.And(regionCondition);
                }

                if (options.CostBlockJoinAdditionalCondition != null)
                {
                    costBlockJoinCondition = costBlockJoinCondition.AndBrackets(options.CostBlockJoinAdditionalCondition);
                }
            }

            foreach (var relatedMeta in costBlockMeta.HistoryMeta.RelatedMetas)
            {
                if (relatedMeta.Name != historyContext.InputLevelId)
                {
                    var joinCondition = SqlOperators.Equals(
                        new ColumnInfo(costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, costBlockMeta.HistoryMeta.Name),
                        new ColumnInfo(relatedMeta.CostBlockHistoryField.Name, relatedMeta.Name));

                    query = query.Join(relatedMeta, joinCondition);

                    var isNullCondition = SqlOperators.IsNull(relatedMeta.RelatedItemField.Name, relatedMeta.Name);
                    var equalCondition = SqlOperators.Equals(
                        new ColumnInfo(relatedMeta.RelatedItemField.Name, relatedMeta.Name),
                        new ColumnInfo(relatedMeta.RelatedItemField.Name, costBlockMeta.Name));

                    costBlockJoinCondition = costBlockJoinCondition.And(
                        ConditionHelper.OrBrackets(isNullCondition, equalCondition));
                }
            }

            return
                query.Join(costBlockMeta.HistoryMeta, costBlockMeta.HistoryMeta.CostBlockHistoryField.Name)
                     .Join(costBlockMeta, costBlockJoinCondition);
        }

        private SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(CostBlockHistory history, TQuery query, IEnumerable<JoinInfo> joinInfos = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(history.Context);
            var costBlockJoinCondition = this.GetHistoryInputLevelJoinCondition(history.Context);

            var options = new JoinHistoryValueQueryOptions
            {
                CostBlockJoinAdditionalCondition = costBlockJoinCondition.ToSqlBuilder(),
                IsUseRegionCondition = true
            };

            query = this.BuildJoinHistoryValueQuery(history.Context, query, options).Join(joinInfos);

            var historyIdCondition =
                SqlOperators.Equals(
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
                    "costBlockHistoryId",
                    history.Id,
                    costBlockMeta.HistoryMeta.Name);

            return query.Where(historyIdCondition);
        }

        private ConditionHelper GetHistoryInputLevelJoinCondition(HistoryContext historyContext)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);

            return SqlOperators.Equals(
                new ColumnInfo(historyContext.InputLevelId, costBlockMeta.HistoryMeta.Name),
                new ColumnInfo(historyContext.InputLevelId, costBlockMeta.Name));
        }

        private IEnumerable<ColumnInfo> GetDependencyColumns(CostBlockEntityMeta costBlockMeta)
        {
            foreach (var dependecyField in costBlockMeta.DependencyFields)
            {
                yield return new ColumnInfo(dependecyField.Name, costBlockMeta.Name);
                yield return new ColumnInfo(dependecyField.ReferenceFaceField, this.GetAlias(dependecyField.ReferenceMeta), $"{dependecyField.Name}_Name");
            }
        }

        private IEnumerable<JoinInfo> GetDependencyJoinInfos(CostBlockEntityMeta costBlockMeta)
        {
            return costBlockMeta.DependencyFields.Select(field => new JoinInfo
            {
                Meta = costBlockMeta,
                ReferenceFieldName = field.Name,
                JoinedTableAlias = this.GetAlias(field.ReferenceMeta)
            });
        }

        private string GetHistoryRegionColumnName()
        {
            return $"{nameof(CostBlockHistory.Context)}_{nameof(HistoryContext.RegionInputId)}";
        }

        private string GetAlias(BaseEntityMeta meta)
        {
            return $"{meta.Schema}_{meta.Name}";
        }

        private CostBlockValueHistory CostBlockValueHistoryMap(IDataReader reader)
        {
            var dependencies = new Dictionary<string, NamedId>();

            for (var i = 3; i < reader.FieldCount; i = i + 2)
            {
                var dependency = new NamedId
                {
                    Id = reader.GetInt64(i),
                    Name = reader.GetString(i + 1)
                };

                dependencies.Add(reader.GetName(i), dependency);
            }

            return new CostBlockValueHistory
            {
                Value = reader.GetValue(0),
                InputLevel = new NamedId
                {
                    Id = reader.GetInt64(1),
                    Name = reader.GetString(2)
                },
                Dependencies = dependencies
            };
        }

        private CostBlockEntityMeta GetCostBlockEntityMeta(HistoryContext historyContext)
        {
            return (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(historyContext.CostBlockId, historyContext.ApplicationId);
        }

        private string ToLowerFirstLetter(string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }

        //private class JoinInfo
        //{
        //    public BaseEntityMeta Meta { get; set; }

        //    public string ReferenceFieldName { get; set; }

        //    public string Alias { get; set; }
        //}

        private class JoinHistoryValueQueryOptions
        {
            public ISqlBuilder CostBlockJoinAdditionalCondition { get; set; }

            public bool IsUseRegionCondition { get; set; }
        }
    }
}
