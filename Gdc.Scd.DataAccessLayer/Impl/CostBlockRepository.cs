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

            var referenceFieldInfos =
                coordinateFields.Select(field => new
                {
                    Field = field,
                    InnerFields =
                        field.ReferenceMeta.AllFields.OfType<ReferenceFieldMeta>()
                                                     .Where(innerField => costBlockRefMetas.Contains(innerField.ReferenceMeta))
                                                     .ToArray()
                }).ToArray();

            var referenceFields = new List<ReferenceFieldMeta>();
            var selectColumns = new List<ColumnInfo>();

            var innerFieldInfos =
                referenceFieldInfos.SelectMany(
                    fieldInfo => fieldInfo.InnerFields.Select(
                        innerField => new { fieldInfo.Field, InnerField = innerField }))
                                   .ToArray();

            var ignoreRefMetas = new HashSet<BaseEntityMeta>(
                referenceFieldInfos.SelectMany(fieldInfo => fieldInfo.InnerFields)
                                   .Select(field => field.ReferenceMeta));

            foreach (var field in coordinateFields)
            {
                if (ignoreRefMetas.Contains(field.ReferenceMeta))
                {
                    var innerFieldInfo = innerFieldInfos.First(x => x.InnerField.ReferenceMeta == field.ReferenceMeta);

                    selectColumns.Add(new ColumnInfo(innerFieldInfo.InnerField.Name, innerFieldInfo.Field.ReferenceMeta.Name, field.Name));
                }
                else
                {
                    referenceFields.Add(field);
                    selectColumns.Add(new ColumnInfo(field.ReferenceValueField, field.Name, field.Name));
                }
            }

            var fromField = referenceFields[0];

            referenceFields.RemoveAt(0);

            var selectQuery =
                Sql.Select(selectColumns.ToArray())
                   .Into(CoordinateTable)
                   .FromQuery(
                        this.BuildReferenceItemsQuery(fromField, costBlockMeta),
                        fromField.ReferenceMeta.Name);

            return
                referenceFields.Aggregate(
                    selectQuery,
                    (accumulateQuery, field) => this.BuildReferenceJoinQuery(accumulateQuery, field, costBlockMeta));
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

        private SelectJoinSqlHelper BuildReferenceJoinQuery(SelectJoinSqlHelper query, ReferenceFieldMeta field, CostBlockEntityMeta costBlockMeta)
        {
            var joinSubquery = new AliasSqlBuilder
            {
                Alias = field.Name,
                Query = new BracketsSqlBuilder
                {
                    Query = this.BuildReferenceItemsQuery(field, costBlockMeta).ToSqlBuilder()
                }
            };

            return query.Join(joinSubquery, (ISqlBuilder)null, JoinType.Cross);
        }

        private GroupBySqlHelper BuildReferenceItemsQuery(ReferenceFieldMeta field, CostBlockEntityMeta costBlockMeta)
        {
            var fieldQuery = Sql.Select().From(field.ReferenceMeta);
            var conditions = new List<ConditionHelper>();

            if (field.ReferenceMeta is DeactivatableEntityMeta deactivatableMeta)
            {
                conditions.Add(SqlOperators.IsNull(deactivatableMeta.DeactivatedDateTimeField.Name));
            }

            switch(field.ReferenceMeta)
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
    }
}
