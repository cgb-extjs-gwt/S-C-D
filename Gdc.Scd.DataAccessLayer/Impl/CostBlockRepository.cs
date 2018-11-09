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
                                Sql.Except(this.BuildCostBlockReferenceIdsQuery(field), this.BuildReferenceIdsQuery(field)))));

            var condition = CostBlockQueryHelper.BuildNotDeletedCondition(meta).And(deleteCondition.ToSqlBuilder());

            return
                Sql.Update(meta, new ValueUpdateColumnInfo(meta.DeletedDateField.Name, DateTime.UtcNow))
                   .Where(condition);
        }

        private SqlHelper BuildCreateRowsCostBlockQuery(CostBlockEntityMeta costBlockMeta)
        {
            var referenceFields = costBlockMeta.CoordinateFields.ToList();
            var selectColumns =
                referenceFields.Select(field => new ColumnInfo(field.ReferenceValueField, field.Name, field.Name))
                               .ToList()
                               .AsEnumerable();

            var insertFields = referenceFields.Select(field => field.Name).ToArray();

            var wgField = costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName];
            var plaField = costBlockMeta.InputLevelFields[MetaConstants.PlaInputLevelName];

            if (plaField != null && wgField != null)
            {
                selectColumns =
                    selectColumns.Select(
                        column => column.TableName == plaField.Name
                            ? new ColumnInfo($"{nameof(Pla)}{nameof(Wg.Id)}", MetaConstants.WgInputLevelName, plaField.Name)
                            : column);

                referenceFields.Remove(plaField);
            }

            var clusterRegionField = costBlockMeta.InputLevelFields[MetaConstants.ClusterRegionInputLevel];
            var countryField = costBlockMeta.InputLevelFields[MetaConstants.CountryInputLevelName];

            ReferenceFieldMeta fromField = null;

            if (countryField == null)
            {
                fromField = referenceFields[0];

                referenceFields.RemoveAt(0);
            }
            else
            {
                if (clusterRegionField != null)
                {
                    selectColumns =
                        selectColumns.Select(
                            column => column.TableName == clusterRegionField.Name
                                ? new ColumnInfo(nameof(Country.ClusterRegionId), MetaConstants.CountryInputLevelName, MetaConstants.ClusterRegionInputLevel)
                                : column);

                    referenceFields.Remove(clusterRegionField);
                }

                fromField = countryField;

                referenceFields.Remove(countryField);
            }

            var selectQuery = 
                Sql.Select(selectColumns.ToArray())
                   .FromQuery(
                        this.BuildNewReferenceItemsQuery(fromField), 
                        fromField.ReferenceMeta.Name);

            var joinQuery = referenceFields.Aggregate(selectQuery, this.BuildReferenceJoinQuery);

            SqlHelper query;

            if (countryField == null)
            {
                query = joinQuery;
            }
            else
            {
                query = joinQuery.Where(
                    SqlOperators.Equals(nameof(Country.IsMaster), "isMaster", true, MetaConstants.CountryInputLevelName));
            }

            return Sql.Insert(costBlockMeta, insertFields).Query(query);
        }

        private SelectJoinSqlHelper BuildReferenceJoinQuery(SelectJoinSqlHelper query, ReferenceFieldMeta field)
        {
            var joinSubquery = new AliasSqlBuilder
            {
                Alias = field.Name,
                Query = new BracketsSqlBuilder
                {
                    Query = this.BuildNewReferenceItemsQuery(field).ToSqlBuilder()
                }
            };

            return query.Join(joinSubquery, (ISqlBuilder)null, JoinType.Cross);
        }

        private GroupBySqlHelper BuildNewReferenceItemsQuery(ReferenceFieldMeta field)
        {
            return
                Sql.Select()
                   .From(field.ReferenceMeta)
                   .Where(
                        SqlOperators.In(
                            MetaConstants.IdFieldKey,
                            Sql.Except(this.BuildReferenceIdsQuery(field), this.BuildCostBlockReferenceIdsQuery(field))));
        }

        private SelectJoinSqlHelper BuildCostBlockReferenceIdsQuery(ReferenceFieldMeta field)
        {
            return 
                Sql.Select(MetaConstants.IdFieldKey)
                   .From(this.BuildCostBlockReferenceIdsAlias(field));
        }

        private GroupBySqlHelper BuildReferenceIdsQuery(ReferenceFieldMeta field)
        {
            var fieldQuery = Sql.Select(field.ReferenceValueField).From(field.ReferenceMeta);
            var conditions = new List<ConditionHelper>();

            if (field.ReferenceMeta is DeactivatableEntityMeta deactivatableMeta)
            {
                conditions.Add(SqlOperators.IsNull(deactivatableMeta.DeactivatedDateTimeField.Name));
            }

            if (field.Name == MetaConstants.WgInputLevelName)
            {
                conditions.Add(SqlOperators.Equals(nameof(Wg.WgType), "wgType", (int)WgType.Por));
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
