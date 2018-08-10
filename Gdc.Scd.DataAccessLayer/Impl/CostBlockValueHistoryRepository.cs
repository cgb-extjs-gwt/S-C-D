using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
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

        private readonly IRepositorySet repositorySet;

        public CostBlockValueHistoryRepository(DomainEnitiesMeta domainEnitiesMeta, IRepositorySet repositorySet)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.repositorySet = repositorySet;
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

            foreach (var dependecyField in costBlockMeta.DependencyFields)
            {
                selectColumns.Add(new ColumnInfo(dependecyField.Name, costBlockMeta.Name));
                selectColumns.Add(new ColumnInfo(dependecyField.ReferenceFaceField, this.GetAlias(dependecyField.ReferenceMeta), $"{dependecyField.Name}_Name"));
            }

            var selectQuery = this.BuildSelectHistoryValueQuery(history.Context, selectColumns);
            var joinInfos = costBlockMeta.DependencyFields.Select(field => new JoinInfo
            {
                Meta = costBlockMeta,
                ReferenceFieldName = field.Name,
                Alias = this.GetAlias(field.ReferenceMeta)
            }).ToList();

            joinInfos.Add(new JoinInfo
            {
                Meta = costBlockMeta.HistoryMeta,
                ReferenceFieldName = history.Context.InputLevelId,
                Alias = inputLevelAlias
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
            var costBlockHistoryAlias = this.GetAlias(costBlockMeta.HistoryMeta.CostBlockHistoryField.ReferenceMeta);

            var historyEditUserIdColumnName = $"{nameof(CostBlockHistory.EditUser)}{nameof(User.Id)}";
            var histroryEditUserIdColumn = new ColumnInfo(historyEditUserIdColumnName, costBlockHistoryAlias, nameof(CostBlockHistoryValueDto.EditUserId));
            var userIdColumn = new ColumnInfo(nameof(User.Id), nameof(User));

            var editDateColumn = new ColumnInfo(nameof(CostBlockHistory.EditDate), costBlockHistoryAlias, nameof(CostBlockHistoryValueDto.EditDate));
            var userNameColumn = new ColumnInfo(nameof(User.Name), nameof(User), nameof(CostBlockHistoryValueDto.EditUserName));

            var selectColumns = new List<ColumnInfo>
            {
                editDateColumn,
                histroryEditUserIdColumn,
                userNameColumn
            };
            
            var selectQuery =
                this.BuildSelectHistoryValueQuery(historyContext, selectColumns)
                    .Join(costBlockMeta.HistoryMeta, costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, costBlockHistoryAlias)
                    .Join(nameof(User),  SqlOperators.Equals(histroryEditUserIdColumn, userIdColumn));

            if (queryInfo == null)
            {
                queryInfo = new QueryInfo();
            }

            if (queryInfo.Sort == null)
            {
                queryInfo.Sort = new SortInfo
                {
                    Direction = SortDirection.Desc,
                    Property = nameof(CostBlockHistoryValueDto.EditDate)
                };
            }

            var query =
                this.BuildJoinHistoryValueQuery(historyContext, selectQuery)
                    .Where(filter, costBlockMeta.Name)
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

        private TQuery BuildJoinHistoryValueQuery<TQuery>(HistoryContext historyContext, TQuery query, ISqlBuilder costBlockJoinAdditionalCondition = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);

            foreach (var relatedMeta in costBlockMeta.HistoryMeta.RelatedMetas)
            {
                var joinCondition = SqlOperators.Equals(
                    new ColumnInfo(costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, costBlockMeta.HistoryMeta.Name),
                    new ColumnInfo(relatedMeta.CostBlockHistoryField.Name, relatedMeta.Name));

                query = query.Join(relatedMeta, joinCondition);
            }

            var costBlockJoinCondition = ConditionHelper.And(costBlockMeta.HistoryMeta.RelatedMetas.Select(relatedMeta =>
            {
                var isNullCondition = SqlOperators.IsNull(relatedMeta.RelatedItemField.Name, relatedMeta.Name);
                var equalCondition = SqlOperators.Equals(
                    new ColumnInfo(relatedMeta.RelatedItemField.Name, relatedMeta.Name),
                    new ColumnInfo(relatedMeta.RelatedItemField.Name, costBlockMeta.Name));

                return ConditionHelper.OrBrackets(isNullCondition, equalCondition).ToSqlBuilder();
            }));

            var createdDateCondition = 
                SqlOperators.LessOrEqual(
                    new ColumnInfo(costBlockMeta.CreatedDateField.Name, costBlockMeta.Name), 
                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName));

            var deletedDateCondition = ConditionHelper.OrBrackets(
                SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                SqlOperators.GreaterOrEqual(new ColumnInfo(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName)));

            costBlockJoinCondition = costBlockJoinCondition.And(createdDateCondition).And(deletedDateCondition);

            if (costBlockJoinAdditionalCondition != null)
            {
                costBlockJoinCondition = costBlockJoinCondition.And(costBlockJoinAdditionalCondition);
            }

            return
                query.Join(costBlockMeta.HistoryMeta, costBlockMeta.HistoryMeta.CostBlockHistoryField.Name)
                     .Join(costBlockMeta, costBlockJoinCondition);
        }

        private SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(CostBlockHistory history, TQuery query, IEnumerable<JoinInfo> joinInfos = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(history.Context);
            var costBlockJoinCondition = SqlOperators.Equals(
                   new ColumnInfo(history.Context.InputLevelId, costBlockMeta.HistoryMeta.Name),
                   new ColumnInfo(history.Context.InputLevelId, costBlockMeta.Name));

            query = this.BuildJoinHistoryValueQuery(history.Context, query, costBlockJoinCondition.ToSqlBuilder());

            if (joinInfos != null)
            {
                foreach (var joinInfo in joinInfos)
                {
                    query = query.Join(joinInfo.Meta, joinInfo.ReferenceFieldName, joinInfo.Alias);
                }
            }
            
            var historyIdCondition =
                SqlOperators.Equals(
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
                    "costBlockHistoryId",
                    history.Id,
                    costBlockMeta.HistoryMeta.Name);

            return query.Where(historyIdCondition);
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

        private class JoinInfo
        {
            public BaseEntityMeta Meta { get; set; }

            public string ReferenceFieldName { get; set; }

            public string Alias { get; set; }
        }
    }
}
