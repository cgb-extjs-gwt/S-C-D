using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockRepository : ICostBlockRepository
    {
        private const string CoordinateTable = "#Coordinates";

        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public CostBlockRepository(
            IRepositorySet repositorySet, 
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.repositorySet = repositorySet;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<int> Update(IEnumerable<EditInfo> editInfos)
        {
            var queries = new List<SqlHelper>();

            foreach (var editInfoGroup in editInfos.GroupBy(editInfo => editInfo.Meta))
            {
                var metaQueries = new List<SqlHelper>();
                var valueInfoGroups =
                    editInfoGroup.SelectMany(editInfo => editInfo.ValueInfos)
                                 .GroupBy(GetKey);

                foreach (var valueInfoGroup in valueInfoGroups)
                {
                    var firstValueInfo = valueInfoGroup.First();
                    var updateColumns = firstValueInfo.Values.Select(costElementValue => new ValueUpdateColumnInfo(
                        costElementValue.Key,
                        costElementValue.Value));

                    var filterConditions = valueInfoGroup.Select(valueInfo => new BracketsSqlBuilder
                    {
                        Query = 
                            ConditionHelper.AndStatic(valueInfo.CoordinateFilter.Convert(), editInfoGroup.Key.Name)
                                           .ToSqlBuilder()
                    });

                    var whereCondition = 
                        CostBlockQueryHelper.BuildNotDeletedCondition(editInfoGroup.Key, editInfoGroup.Key.Name)
                                            .AndBrackets(ConditionHelper.Or(filterConditions));

                    var query =
                        Sql.Update(editInfoGroup.Key, updateColumns.ToArray())
                           .Where(whereCondition);

                    metaQueries.Add(query);
                }

                if (editInfoGroup.Key.SliceDomainMeta.DisableTriggers && metaQueries.Count > 1)
                {
                    metaQueries.Insert(0, Sql.DisableTriggers(editInfoGroup.Key));
                    metaQueries.Insert(metaQueries.Count - 1, Sql.EnableTriggers(editInfoGroup.Key));
                }

                queries.AddRange(metaQueries);
            }

            var result = 0;

            if (queries.Count > 0)
            {
                result = await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
            }

            return result;

            string GetKey(ValuesInfo valuesInfo)
            {
                var values =
                    valuesInfo.Values.OrderBy(keyValue => keyValue.Key)
                                     .Select(keyValue => $"{keyValue.Key}:{keyValue.Value}");

                return string.Join(",", values);
            }
        }

        public async Task<int> UpdateByCoordinatesAsync(CostBlockEntityMeta meta,
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            var query = this.BuildUpdateByCoordinatesQuery(meta, updateOptions);

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        public void UpdateByCoordinates(CostBlockEntityMeta meta,
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            var query = this.BuildUpdateByCoordinatesQuery(meta, updateOptions);
            this.repositorySet.ExecuteSql(query);
        }

        public void CreatRegionIndexes()
        {
            var costblockInfos =
                this.domainEnitiesMeta.CostBlocks.Select(costBlock => new
                {
                    CostBlock = costBlock,
                    RegionIds = 
                        costBlock.SliceDomainMeta.CostElements.Where(costElement => costElement.RegionInput != null)
                                                              .GroupBy(costElement => costElement.RegionInput.Id)
                                                              .Select(group => group.Key)
                                                              .ToArray()
                });

            var queries = new List<ISqlBuilder>();

            foreach (var costBlockInfo in costblockInfos.Where(info => info.RegionIds.Length > 0))
            {
                foreach (var regionId in costBlockInfo.RegionIds)
                {
                    var costBlock = costBlockInfo.CostBlock;

                    queries.Add(new CreateIndexSqlBuilder
                    {
                        Name = $"IX_{costBlock.Schema}_{costBlock.Name}_{regionId}",
                        Schema = costBlock.Schema,
                        Table = costBlock.Name,
                        Columns = new[]
                        {
                            new IndexColumn
                            {
                                ColumnName = regionId,
                            }
                        }
                    });
                }
            }

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }

        public SqlHelper BuildUpdateByCoordinatesQuery(
            CostBlockEntityMeta meta,
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            var queries = new List<SqlHelper>();

            if (updateOptions != null)
            {
                foreach (var updateOption in updateOptions)
                {
                    if (updateOption.OldCoordinates.Count != updateOption.NewCoordinates.Count ||
                        updateOption.OldCoordinates.Count != updateOption.OldCoordinates.Keys.Intersect(updateOption.NewCoordinates.Keys).Count())
                    {
                        throw new Exception($"{nameof(UpdateQueryOption.OldCoordinates)} don't match {nameof(UpdateQueryOption.NewCoordinates)}");
                    }

                    if (updateOption.OldCoordinates.Keys.All(meta.ContainsCoordinateField))
                    {
                        queries.Add(this.BuildUpdateCostBlockByUpdateOptionQuery(meta, updateOption));
                    }
                }
            }

            queries.AddRange(new[]
            {
                this.BuildCoordinatesTableQuery(meta),
                this.BuildCreateRowsCostBlockQuery(meta),
                this.BuildDeleteRowsCostBlockQuery(meta),
                Sql.DropTable(CoordinateTable)
            });

            return Sql.Queries(queries.ToArray());
        }

        public async Task<NamedId[]> GetDependencyByPortfolio(CostElementContext context)
        {
            var costBlock = this.domainEnitiesMeta.CostBlocks[context];
            var regionField = costBlock.GetDomainRegionInputField(context.CostElementId);
            var regionMeta = regionField.ReferenceMeta;
            var dependencyField = costBlock.GetDomainDependencyField(context.CostElementId);
            var dependencyMeta = (NamedEntityMeta)dependencyField.ReferenceMeta;

            var selectIdsQuery = Sql.Union(new[]
            {
                BuildSelectIdsQuery(this.domainEnitiesMeta.LocalPortfolio),
                BuildSelectIdsQuery(this.domainEnitiesMeta.HwStandardWarranty)
            });

            var query =
                Sql.Select(dependencyMeta.IdField.Name, dependencyMeta.NameField.Name)
                   .From(dependencyField.ReferenceMeta)
                   .Where(SqlOperators.In(dependencyMeta.IdField.Name, selectIdsQuery, dependencyMeta.Name));

            var items = await this.repositorySet.ReadBySqlAsync(query, reader =>
            {
                return new NamedId
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1)
                };
            });

            return items.ToArray();

            SqlHelper BuildSelectIdsQuery(BaseEntityMeta meta)
            {
                var selectField = meta.GetFieldByReferenceMeta(dependencyMeta);
                var conditions = new List<ConditionHelper>();

                if (context.RegionInputId.HasValue)
                {
                    var field = meta.GetFieldByReferenceMeta(regionMeta);
                    if (field != null)
                    {
                        conditions.Add(SqlOperators.Equals(field.Name, context.RegionInputId.Value));
                    }
                }

                return
                    Sql.SelectDistinct(new ColumnInfo(selectField.Name, alias: MetaConstants.IdFieldKey))
                       .From(meta)
                       .Where(ConditionHelper.And(conditions));
            }
        }

        private SqlHelper BuildCoordinatesTableQuery(CostBlockEntityMeta costBlockMeta)
        {
            var coordinateFields = costBlockMeta.CoordinateFields.ToArray();
            var costBlockRefMetas = new HashSet<BaseEntityMeta>(coordinateFields.Select(field => field.ReferenceMeta));

            var relatedFieldInfos =
                coordinateFields.SelectMany(BuildRelatedFieldInfos)
                                .GroupBy(info => info.RelatedField.ReferenceMeta)
                                .ToDictionary(group => group.Key, group => group.First());

            var selectColumns = new List<ColumnInfo>();
            var referenceMetas = new List<BaseEntityMeta>();
            var joinInfos = new List<ReferenceJoinInfo>();

            foreach (var coordinateField in coordinateFields)
            {
                if (relatedFieldInfos.TryGetValue(coordinateField.ReferenceMeta, out var relatedFieldInfo))
                {
                    selectColumns.Add(new ColumnInfo(relatedFieldInfo.RelatedField.Name, relatedFieldInfo.RelatedMeta.Name, coordinateField.Name));

                    var cycleCheckCollection = new HashSet<BaseEntityMeta>();
                    var relatedMeta = relatedFieldInfo.RelatedMeta;

                    while (!cycleCheckCollection.Contains(relatedMeta) && relatedFieldInfos.TryGetValue(relatedMeta, out relatedFieldInfo))
                    {
                        cycleCheckCollection.Add(relatedMeta);

                        if (joinInfos.All(joinInfo => joinInfo.ReferenceMeta != relatedFieldInfo.RelatedField.ReferenceMeta))
                        {
                            joinInfos.Add(new ReferenceJoinInfo
                            {
                                ReferenceMeta = relatedFieldInfo.RelatedField.ReferenceMeta,
                                InnerJoinInfo = new JoinInfo(relatedFieldInfo.RelatedMeta, relatedFieldInfo.RelatedField.Name)
                            });
                        }

                        relatedMeta = relatedFieldInfo.RelatedMeta;
                    }
                }
                else
                {
                    referenceMetas.Add(coordinateField.ReferenceMeta);
                    selectColumns.Add(new ColumnInfo(coordinateField.ReferenceValueField, coordinateField.ReferenceMeta.Name, coordinateField.Name));
                }
            }

            var fromMeta = referenceMetas[0];
            var selectQuery =
                Sql.Select(selectColumns.ToArray())
                   .Into(CoordinateTable)
                   .FromQuery(
                        this.BuildReferenceItemsQuery(fromMeta, costBlockMeta),
                        fromMeta.Name);

            for (var i = 0; i < joinInfos.Count - 1; i++)
            {
                for (var j = i + 1; j < joinInfos.Count; j++)
                {
                    if(joinInfos[i].InnerJoinInfo.Meta.FullName == joinInfos[j].ReferenceMeta.FullName)
                    {
                        var tmp = joinInfos[i];
                        joinInfos[i] = joinInfos[j];
                        joinInfos[j] = tmp;
                    }
                }
            }

            return
                referenceMetas.Skip(1)
                              .Select(meta => new ReferenceJoinInfo { ReferenceMeta = meta })
                              .Concat(joinInfos)
                              .Aggregate(
                                  selectQuery,
                                  (accumulateQuery, joinInfo) => this.BuildReferenceJoinQuery(accumulateQuery, joinInfo, costBlockMeta));

            IEnumerable<(ReferenceFieldMeta RelatedField, BaseEntityMeta RelatedMeta)> BuildRelatedFieldInfos(FieldMeta field)
            {
                if (field is ReferenceFieldMeta refField)
                {
                    foreach (var innerField in refField.ReferenceMeta.AllFields)
                    {
                        switch (innerField)
                        {
                            case ReferenceFieldMeta innerRefField:
                                if (costBlockRefMetas.Contains(innerRefField.ReferenceMeta))
                                {
                                    yield return (innerRefField, refField.ReferenceMeta);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private SqlHelper BuildCreateRowsCostBlockQuery(CostBlockEntityMeta costBlockMeta)
        {
            const string NewRowsTableName = "NewRows";
            const string ValuesTableName = "Values";

            var lastInputLevel = costBlockMeta.SliceDomainMeta.InputLevels.Last();
            var valuesQueryCoordinateFields = 
                costBlockMeta.CoordinateFields.Where(field => field.Name != lastInputLevel.Caption)
                                              .ToArray();

            var insertedFieldNames = 
                costBlockMeta.CoordinateFields.Concat(costBlockMeta.CostElementsFields)
                                              .Select(field => field.Name)
                                              .ToArray();

            var selectedColumns =
                costBlockMeta.CoordinateFields.Select(field => new ColumnInfo(field.Name, NewRowsTableName))
                                              .Concat(costBlockMeta.CostElementsFields.Select(field => new ColumnInfo(field.Name, ValuesTableName)))
                                              .ToArray();

            return
                 Sql.Insert(costBlockMeta, insertedFieldNames)
                    .Query(
                        Sql.Select(selectedColumns)
                           .FromQuery(
                                Sql.Except(
                                    this.BuildSelectFromCoordinateTalbeQuery(costBlockMeta),
                                    this.BuildSelectFromCostBlockQuery(costBlockMeta)),
                                NewRowsTableName)
                           .Join(
                                new AliasSqlBuilder
                                {
                                    Alias = ValuesTableName,
                                    Query = new BracketsSqlBuilder
                                    {
                                        Query = BuildValuesQuery()
                                    } 
                                },
                                ConditionHelper.And(
                                    valuesQueryCoordinateFields.Select(
                                        field => SqlOperators.Equals(
                                            new ColumnInfo(field.Name, NewRowsTableName), 
                                            new ColumnInfo(field.Name, ValuesTableName)))),
                                JoinType.Left));

            ISqlBuilder BuildValuesQuery()
            {
                const string MinMaxValuesTableName = "MinMaxValues";

                var groupByColumns = valuesQueryCoordinateFields.Select(field => new ColumnInfo(field.Name)).ToArray();

                var minMaxColumnInfos = costBlockMeta.CostElementsFields.Select(field => new
                {
                    FieldName = field.Name,
                    MinColumn = SqlFunctions.Min(field, alias: $"{field.Name}Min"),
                    MaxColumn = SqlFunctions.Max(field, alias: $"{field.Name}Max")
                }).ToArray();

                var minMaxSelectColumns =
                    valuesQueryCoordinateFields.Select(field => new ColumnInfo(field.Name))
                                               .Cast<BaseColumnInfo>()
                                               .Concat(minMaxColumnInfos.SelectMany(info => new[] { info.MinColumn, info.MaxColumn }))
                                               .ToArray();

                var valueColumns = minMaxColumnInfos.Select(info => new QueryColumnInfo(
                    new CaseSqlBuilder
                    {
                        Cases = new[]
                        {
                            new CaseItem
                            {
                                When =
                                    SqlOperators.Equals(
                                        new ColumnInfo(info.MinColumn.Alias, MinMaxValuesTableName),
                                        new ColumnInfo(info.MaxColumn.Alias, MinMaxValuesTableName))
                                                .ToSqlBuilder(),
                                Then = new ColumnSqlBuilder(info.MinColumn.Alias, MinMaxValuesTableName)
                            }
                        },
                        Else = new RawSqlBuilder("NULL")
                    },
                    info.FieldName));

                var selectColumns =
                    valuesQueryCoordinateFields.Select(field => new ColumnInfo(field.Name, MinMaxValuesTableName))
                                               .Cast<BaseColumnInfo>()
                                               .Concat(valueColumns)
                                               .ToArray();

                return
                    Sql.Select(selectColumns)
                       .FromQuery(
                            Sql.Select(minMaxSelectColumns)
                               .From(costBlockMeta)
                               .WhereNotDeleted(costBlockMeta)
                               .GroupBy(groupByColumns),
                            MinMaxValuesTableName)
                       .ToSqlBuilder();
            }
        }

        private SqlHelper BuildUpdateCostBlockByUpdateOptionQuery(CostBlockEntityMeta costBlock, UpdateQueryOption updateOption)
        {
            var oldCoordinatesFilter =
                updateOption.OldCoordinates.ToDictionary(
                    keyValue => keyValue.Key,
                    keyValue => new object[] { keyValue.Value } as IEnumerable<object>);

            return Sql.Queries(new SqlHelper[]
            {
                BuildInsertOldCoordinatesRowsQuery(),
                BuildUpdateCoordinatesQuery()
            });

            T BuildGetOldCoordinatesQuery<T>(IFromSqlHelper<IWhereSqlHelper<T>> query)
            {
                return
                    query.From(costBlock)
                         .WhereNotDeleted(costBlock, oldCoordinatesFilter);
            }

            SqlHelper BuildInsertOldCoordinatesRowsQuery()
            {
                var costBlockFieldNames =
                    costBlock.AllFields.Where(field => field != costBlock.IdField)
                                       .Select(field => field.Name)
                                       .ToArray();

                return
                    Sql.Insert(costBlock, costBlockFieldNames)
                       .Query(
                            BuildGetOldCoordinatesQuery(
                                Sql.Select(costBlockFieldNames.Select(BuildNewValueColumn).ToArray())));

                BaseColumnInfo BuildNewValueColumn(string fieldName)
                {
                    BaseColumnInfo column;

                    if (fieldName == costBlock.ActualVersionField.Name)
                    {
                        column = new ColumnInfo(costBlock.IdField.Name, null, fieldName);
                    }
                    else if (fieldName == costBlock.DeletedDateField.Name)
                    {
                        column = new QueryColumnInfo(new ParameterSqlBuilder(DateTime.UtcNow), fieldName);
                    }
                    else
                    {
                        column = new ColumnInfo(fieldName);
                    }

                    return column;
                }
            }

            SqlHelper BuildUpdateCoordinatesQuery()
            {
                var updateColumns =
                    updateOption.NewCoordinates.Select(coord => new ValueUpdateColumnInfo(coord.Key, coord.Value))
                                               .Cast<BaseUpdateColumnInfo>()
                                               .Concat(new BaseUpdateColumnInfo[]
                                               {
                                                   new QueryUpdateColumnInfo(
                                                       costBlock.ActualVersionField.Name, 
                                                       new ColumnSqlBuilder(costBlock.IdField.Name))
                                               })
                                               .ToArray();

                return BuildGetOldCoordinatesQuery(
                    Sql.Update(costBlock, updateColumns));
            }
        }

        private SqlHelper BuildDeleteRowsCostBlockQuery(CostBlockEntityMeta costBlockMeta)
        {
            const string DeletedCoordinateTable = "DeletedCoordinate";

            var condition =
                ConditionHelper.And(
                    costBlockMeta.CoordinateFields.Select(
                        field => 
                            SqlOperators.Equals(
                                new ColumnInfo(field.Name, DeletedCoordinateTable), 
                                new ColumnInfo(field.Name, costBlockMeta.Name))));

            return
                Sql.Update(costBlockMeta, new ValueUpdateColumnInfo(costBlockMeta.DeletedDateField.Name, DateTime.UtcNow))
                   .FromQuery(
                        Sql.Except(
                            this.BuildSelectFromCostBlockQuery(costBlockMeta),
                            this.BuildSelectFromCoordinateTalbeQuery(costBlockMeta)),
                        DeletedCoordinateTable)
                   .Where(condition);
        }

        private SelectJoinSqlHelper BuildSelectFromCoordinateTalbeQuery(CostBlockEntityMeta costBlockMeta)
        {
            return Sql.Select(costBlockMeta.CoordinateFields).From(CoordinateTable);
        }

        private GroupBySqlHelper BuildSelectFromCostBlockQuery(CostBlockEntityMeta costBlockMeta)
        {
            return Sql.Select(costBlockMeta.CoordinateFields).From(costBlockMeta).WhereNotDeleted(costBlockMeta);
        }

        private SelectJoinSqlHelper BuildReferenceJoinQuery(SelectJoinSqlHelper query, ReferenceJoinInfo referenceJoinInfo, CostBlockEntityMeta costBlockMeta)
        {
            var joinSubquery = new AliasSqlBuilder
            {
                Alias = referenceJoinInfo.ReferenceMeta.Name,
                Query = new BracketsSqlBuilder
                {
                    Query = this.BuildReferenceItemsQuery(referenceJoinInfo.ReferenceMeta, costBlockMeta).ToSqlBuilder()
                }
            };

            ConditionHelper joinCondition;
            JoinType joinType;

            if (referenceJoinInfo.InnerJoinInfo == null)
            {
                joinType = JoinType.Cross;
                joinCondition = null;
            }
            else
            {
                var referenceField = 
                    (ReferenceFieldMeta)referenceJoinInfo.InnerJoinInfo.Meta.GetField(referenceJoinInfo.InnerJoinInfo.ReferenceFieldName);

                joinType = JoinType.Inner;
                joinCondition = SqlOperators.Equals(
                    new ColumnInfo(referenceField.Name, referenceJoinInfo.InnerJoinInfo.Meta.Name),
                    new ColumnInfo(referenceField.ReferenceValueField, referenceField.ReferenceMeta.Name));
            }

            return query.Join(joinSubquery, joinCondition, joinType);
        }

        private GroupBySqlHelper BuildReferenceItemsQuery(BaseEntityMeta referenceMeta, CostBlockEntityMeta costBlockMeta)
        {
            var fieldQuery = Sql.Select().From(referenceMeta);
            var conditions = new List<ConditionHelper>();

            if (referenceMeta is DeactivatableEntityMeta deactivatableMeta)
            {
                conditions.Add(SqlOperators.IsNull(deactivatableMeta.DeactivatedDateTimeField.Name));
            }

            switch(referenceMeta)
            {
                case WgEnityMeta wgMeta:
                    if (costBlockMeta.Name != MetaConstants.AvailabilityFeeCountryWgCostBlock &&
                        costBlockMeta.Name != MetaConstants.AvailabilityFeeWgCostBlock)
                    {
                        conditions.Add(SqlOperators.Equals(wgMeta.WgTypeField.Name, (int)WgType.Por));
                    }

                    conditions.Add(SqlOperators.Equals(wgMeta.IsSoftwareField.Name, costBlockMeta.Schema == MetaConstants.SoftwareSolutionSchema));
                    break;

                case CountryEntityMeta countryMeta:
                    conditions.Add(SqlOperators.Equals(countryMeta.IsMasterField.Name, true));
                    break;
            }

            return conditions.Count == 0
                ? fieldQuery
                : fieldQuery.Where(ConditionHelper.And(conditions));
        }

        private class ReferenceJoinInfo
        {
            public BaseEntityMeta ReferenceMeta { get; set; }

            public JoinInfo InnerJoinInfo { get; set; }
        }
    }
}
