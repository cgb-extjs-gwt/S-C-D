using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public CostBlockRepository(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public async Task<int> UpdateByCoordinatesAsync(CostBlockEntityMeta meta)
        {
            var query = this.BuildUpdateByCoordinatesQuery(meta);

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        public void UpdateByCoordinates(CostBlockEntityMeta meta)
        {
            var query = this.BuildUpdateByCoordinatesQuery(meta);

            this.repositorySet.ExecuteSql(query);
        }

        private SqlHelper BuildUpdateByCoordinatesQuery(CostBlockEntityMeta meta)
        {
            return Sql.Queries(new[] 
            {
                this.BuildCoordinatesTableQuery(meta),
                this.BuildCreateRowsCostBlockQuery(meta),
                this.BuildDeleteRowsCostBlockQuery(meta),
                Sql.DropTable(CoordinateTable)
            });
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

                        if (joinInfos.All(joinInfo => joinInfo.ReferenceMeta != relatedFieldInfo.RelatedMeta))
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
            var coordinateFieldNames = costBlockMeta.CoordinateFields.Select(field => field.Name).ToArray();

            return
                 Sql.Insert(costBlockMeta, coordinateFieldNames)
                    .Query(
                        Sql.Except(
                            this.BuildSelectFromCoordinateTalbeQuery(costBlockMeta),
                            this.BuildSelectFromCostBlockQuery(costBlockMeta)));
        }

        private SqlHelper BuildDeleteRowsCostBlockQuery(CostBlockEntityMeta costBlockMeta)
        {
            const string DelectedCoordinateTable = "DelectedCoordinate";

            var condition =
                ConditionHelper.And(
                    costBlockMeta.CoordinateFields.Select(
                        field => 
                            SqlOperators.Equals(
                                new ColumnInfo(field.Name, DelectedCoordinateTable), 
                                new ColumnInfo(field.Name, costBlockMeta.Name))));

            return
                Sql.Update(costBlockMeta, new ValueUpdateColumnInfo(costBlockMeta.DeletedDateField.Name, DateTime.UtcNow))
                   .FromQuery(
                        Sql.Except(
                            this.BuildSelectFromCostBlockQuery(costBlockMeta),
                            this.BuildSelectFromCoordinateTalbeQuery(costBlockMeta)),
                        DelectedCoordinateTable)
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
                    conditions.Add(SqlOperators.Equals(wgMeta.WgTypeField.Name, "wgType", (int)WgType.Por));
                    conditions.Add(SqlOperators.Equals(wgMeta.IsSoftwareField.Name, "isSoftware", costBlockMeta.Schema == MetaConstants.SoftwareSolutionSchema));
                    break;

                case CountryEntityMeta countryMeta:
                    conditions.Add(SqlOperators.Equals(countryMeta.IsMasterField.Name, "isMaster", true));
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
