using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryRepository : ICostBlockValueHistoryRepository
    {
        private readonly DomainMeta domainMeta;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockValueHistoryQueryBuilder historyQueryBuilder;

        public CostBlockValueHistoryRepository(
            DomainMeta domainMeta, 
            IRepositorySet repositorySet, 
            ICostBlockValueHistoryQueryBuilder costBlockValueHistoryQueryBuilder)
        {
            this.repositorySet = repositorySet;
            this.domainMeta = domainMeta;
            this.historyQueryBuilder = costBlockValueHistoryQueryBuilder;
        }

        public async Task Save(CostBlockHistory history, IEnumerable<EditItem> editItems, IDictionary<string, long[]> relatedItems)
        {
            var costBlockMeta = historyQueryBuilder.GetCostBlockEntityMeta(history.Context);
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
            var costBlockMeta = historyQueryBuilder.GetCostBlockEntityMeta(history.Context);
            var inputLevelField = costBlockMeta.InputLevelFields[history.Context.InputLevelId];
            var inputLevelMeta = (NamedEntityMeta)inputLevelField.ReferenceMeta;

            var inputLevelIdColumn = new ColumnInfo(inputLevelMeta.IdField.Name, costBlockMeta.HistoryMeta.Name, "InputLevelId");
            var inputLevelAlias = historyQueryBuilder.GetAlias(costBlockMeta.HistoryMeta);
            var inputLevelNameColumn = new ColumnInfo(inputLevelMeta.NameField.Name, inputLevelAlias, "InputLevelName");

            var selectColumns = new List<ColumnInfo> { inputLevelIdColumn, inputLevelNameColumn };
            selectColumns.AddRange(this.GetDependencyColumns(costBlockMeta));

            var selectQuery = historyQueryBuilder.BuildSelectHistoryValueQuery(history.Context, selectColumns);
            var joinInfos = this.GetDependencyJoinInfos(costBlockMeta).ToList();

            joinInfos.Add(new JoinInfo
            {
                Meta = costBlockMeta.HistoryMeta,
                ReferenceFieldName = history.Context.InputLevelId,
                JoinedTableAlias = inputLevelAlias
            });

            var query = historyQueryBuilder.BuildJoinApproveHistoryValueQuery(history, selectQuery, joinInfos);

            return await this.repositorySet.ReadBySql(query, this.CostBlockValueHistoryMap);
        }

        public async Task<IEnumerable<CostBlockHistoryValueDto>> GetCostBlockHistoryValueDto(
            HistoryContext historyContext, 
            IDictionary<string, IEnumerable<object>> filter,
            QueryInfo queryInfo = null)
        {
            var costBlockMeta = historyQueryBuilder.GetCostBlockEntityMeta(historyContext);

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
            
            var selectQuery = historyQueryBuilder.BuildSelectHistoryValueQuery(historyContext, selectColumns);

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

            var costElement = this.domainMeta.CostBlocks[historyContext.CostBlockId].CostElements[historyContext.CostElementId];
            var whereCondition =
                ConditionHelper.AndStatic(filter, costBlockMeta.Name)
                               .And(SqlOperators.IsNotNull(costElement.Id, costBlockMeta.HistoryMeta.Name));

            var userIdColumn = new ColumnInfo(nameof(User.Id), nameof(User));
            var options = new JoinHistoryValueQueryOptions
            {
                IsUseRegionCondition = true,
                InputLevelJoinType = InputLevelJoinType.All
            };

            var query =
                historyQueryBuilder.BuildJoinHistoryValueQuery(historyContext, selectQuery, options)
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
            var costBlockMeta = historyQueryBuilder.GetCostBlockEntityMeta(history.Context);
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

            var query = historyQueryBuilder.BuildJoinApproveHistoryValueQuery(history, updateQuery);

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        private IEnumerable<ColumnInfo> GetDependencyColumns(CostBlockEntityMeta costBlockMeta)
        {
            foreach (var dependecyField in costBlockMeta.DependencyFields)
            {
                yield return new ColumnInfo(dependecyField.Name, costBlockMeta.Name);
                yield return new ColumnInfo(
                    dependecyField.ReferenceFaceField, 
                    historyQueryBuilder.GetAlias(dependecyField.ReferenceMeta), 
                    $"{dependecyField.Name}_Name");
            }
        }

        private IEnumerable<JoinInfo> GetDependencyJoinInfos(CostBlockEntityMeta costBlockMeta)
        {
            return costBlockMeta.DependencyFields.Select(field => new JoinInfo
            {
                Meta = costBlockMeta,
                ReferenceFieldName = field.Name,
                JoinedTableAlias = historyQueryBuilder.GetAlias(field.ReferenceMeta)
            });
        }

        private string GetHistoryRegionColumnName()
        {
            return $"{nameof(CostBlockHistory.Context)}_{nameof(HistoryContext.RegionInputId)}";
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

        private string ToLowerFirstLetter(string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }
    }
}
