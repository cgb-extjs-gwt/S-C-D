using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockRepository : ICostBlockRepository
    {
        private const string CoordinateTable = "#Coordinates";

        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public CostBlockRepository(IRepositorySet repositorySet, DomainEnitiesMeta domainEnitiesMeta)
        {
            this.repositorySet = repositorySet;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<int> Update(IEnumerable<EditInfo> editInfos)
        {
            var queries = new List<SqlHelper>();

            foreach (var editInfo in editInfos)
            {
                var valueInfos = editInfo.ValueInfos.ToArray();

                foreach (var valueInfo in valueInfos)
                {
                    var updateColumns = valueInfo.Values.Select(costElementValue => new ValueUpdateColumnInfo(
                        costElementValue.Key,
                        costElementValue.Value));

                    var query =
                        Sql.Update(editInfo.Meta, updateColumns.ToArray())
                           .WhereNotDeleted(editInfo.Meta, valueInfo.CoordinateFilter.Convert(), editInfo.Meta.Name);

                    queries.Add(query);
                }

                if (valueInfos.Length > 1)
                {
                    queries.Insert(queries.Count - valueInfos.Length, Sql.DisableTriggers(editInfo.Meta));
                    queries.Insert(queries.Count - 1, Sql.EnableTriggers(editInfo.Meta));
                }
            }

            return await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
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
                        costBlock.DomainMeta.CostElements.Where(costElement => costElement.RegionInput != null)
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

        private SqlHelper BuildUpdateByCoordinatesQuery(
            CostBlockEntityMeta meta,
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            List<SqlHelper> queries = new List<SqlHelper>();

            if (updateOptions != null && updateOptions.Any())
            {
                queries.AddRange(this.BuildAllUpdateCostBlockQueriesByCoordinates(meta, updateOptions));
                queries.AddRange(this.BuildAllUpdateCostBlockQueriesByCoordinates(meta.HistoryMeta, updateOptions, "Hist"));
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

            var lastInputLevel = costBlockMeta.DomainMeta.InputLevels.Last();
            var valuesQueryCoordinateFields = 
                costBlockMeta.CoordinateFields.Where(field => field.Name != lastInputLevel.Name)
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

        /// <summary>
        /// Returns all UPDATE queries for Cost Block if any of coordinate is changed
        /// </summary>
        /// <param name="costBlockMeta">Cost Block info</param>
        /// <param name="updateOptions">Updated coordinates with old and new values</param>
        /// <returns></returns>
        private SqlHelper[] BuildAllUpdateCostBlockQueriesByCoordinates(
            BaseCostBlockEntityMeta costBlockMeta,
            IEnumerable<UpdateQueryOption> updateOptions, 
            string prefix = "")
        {
            List<SqlHelper> queries = new List<SqlHelper>();

            var updateQueries = updateOptions.Select((opt, index) => this.BuildUpdateCostBlockByChangedCoordinatesQuery(costBlockMeta, opt, index, prefix))
                                                                                                            .Where(q => q != null);

            queries.AddRange(updateQueries);

            return queries.ToArray();
        }

        private SqlHelper BuildUpdateCostBlockByChangedCoordinatesQuery(
            BaseCostBlockEntityMeta costBlockMeta, 
            UpdateQueryOption updateOptions, 
            int index, 
            string prefix = "")
        {
            var costBlockCoordinates = new HashSet<string>(costBlockMeta.CoordinateFields.Select(c => c.Name));
            var changedCoordinates = new HashSet<string>(updateOptions.NewCoordinates.Keys);

            if (changedCoordinates.IsSubsetOf(costBlockCoordinates))
            {
                var condition = ConditionHelper.And(
                    updateOptions.OldCoordinates.Select(
                        field => SqlOperators.Equals(field.Key, field.Value)));

                var updatedColumns = 
                    updateOptions.NewCoordinates
                                 .Select(field => new ValueUpdateColumnInfo(field.Key, field.Value))
                                 .ToArray();

                return Sql.Update(costBlockMeta, updatedColumns).Where(condition);
            }

            return null;
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
                    if (costBlockMeta.Name != MetaConstants.AvailabilityFeeCostBlock)
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
