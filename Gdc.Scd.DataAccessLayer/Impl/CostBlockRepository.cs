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
            var selectIntoQueries = new List<SqlHelper>();
            var dropTableQueries = new List<SqlHelper>();

            foreach (var field in meta.CoordinateFields)
            {
                var table = this.BuildCostBlockReferenceIdsAlias(field);

                selectIntoQueries.Add(
                    Sql.SelectDistinct(new ColumnInfo(field.Name, null, MetaConstants.IdFieldKey))
                       .Into(table)
                       .From(meta)
                       .WhereNotDeleted(meta));

                dropTableQueries.Add(Sql.DropTable(table));
            }

            var queries = new List<SqlHelper>(selectIntoQueries);

            queries.Add(this.BuildCreateRowsCostBlockQuery(meta));
            queries.Add(this.BuildDeleteRowsCostBlockQuery(meta));
            queries.AddRange(dropTableQueries);

            return Sql.Queries(queries);
        }

        private SqlHelper BuildDeleteRowsCostBlockQuery(CostBlockEntityMeta meta)
        {
            var deleteCondition =
                ConditionHelper.OrBrackets(
                    meta.CoordinateFields.Select(
                        field =>
                            SqlOperators.In(
                                field.Name,
                                Sql.Except(this.BuildCostBlockReferenceIdsQuery(field), this.BuildReferenceIdsQuery(field, meta)))));

            var condition = CostBlockQueryHelper.BuildNotDeletedCondition(meta).And(deleteCondition.ToSqlBuilder());

            return
                Sql.Update(meta, new ValueUpdateColumnInfo(meta.DeletedDateField.Name, DateTime.UtcNow))
                   .Where(condition);
        }

        private SqlHelper BuildCreateRowsCostBlockQuery(CostBlockEntityMeta costBlockMeta)
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


            var insertFields = new List<string>();
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
                insertFields.Add(field.Name);

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
                   .FromQuery(
                        this.BuildNewReferenceItemsQuery(fromField, costBlockMeta), 
                        fromField.ReferenceMeta.Name);

            var joinQuery = 
                referenceFields.Aggregate(
                    selectQuery, 
                    (accumulateQuery, field) => this.BuildReferenceJoinQuery(accumulateQuery, field, costBlockMeta));

            return Sql.Insert(costBlockMeta, insertFields.ToArray()).Query(joinQuery);
        }

        private SelectJoinSqlHelper BuildReferenceJoinQuery(SelectJoinSqlHelper query, ReferenceFieldMeta field, CostBlockEntityMeta costBlockMeta)
        {
            var joinSubquery = new AliasSqlBuilder
            {
                Alias = field.Name,
                Query = new BracketsSqlBuilder
                {
                    Query = this.BuildNewReferenceItemsQuery(field, costBlockMeta).ToSqlBuilder()
                }
            };

            return query.Join(joinSubquery, (ISqlBuilder)null, JoinType.Cross);
        }

        private GroupBySqlHelper BuildNewReferenceItemsQuery(ReferenceFieldMeta field, CostBlockEntityMeta costBlockMeta)
        {
            return
                Sql.Select()
                   .From(field.ReferenceMeta)
                   .Where(
                        SqlOperators.In(
                            MetaConstants.IdFieldKey,
                            Sql.Except(
                                this.BuildReferenceIdsQuery(field, costBlockMeta), 
                                this.BuildCostBlockReferenceIdsQuery(field))));
        }

        private SelectJoinSqlHelper BuildCostBlockReferenceIdsQuery(ReferenceFieldMeta field)
        {
            return 
                Sql.Select(MetaConstants.IdFieldKey)
                   .From(this.BuildCostBlockReferenceIdsAlias(field));
        }

        private GroupBySqlHelper BuildReferenceIdsQuery(ReferenceFieldMeta field, CostBlockEntityMeta costBlockMeta)
        {
            var fieldQuery = Sql.Select(field.ReferenceValueField).From(field.ReferenceMeta);
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

        private string BuildCostBlockReferenceIdsAlias(ReferenceFieldMeta field)
        {
            return $"#{field.ReferenceMeta.Name}_CostBlock";
        }
    }
}
