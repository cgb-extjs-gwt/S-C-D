using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryRepository : ICostBlockValueHistoryRepository
    {
        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockValueHistoryQueryBuilder historyQueryBuilder;

        public CostBlockValueHistoryRepository(
            DomainMeta domainMeta,
            DomainEnitiesMeta domainEnitiesMeta,
            IRepositorySet repositorySet, 
            ICostBlockValueHistoryQueryBuilder costBlockValueHistoryQueryBuilder)
        {
            this.repositorySet = repositorySet;
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.historyQueryBuilder = costBlockValueHistoryQueryBuilder;
        }

        public async Task Save(IEnumerable<HistoryContext> historyContexts)
        {
            var queries = new List<SqlHelper>();

            foreach (var historyContext in historyContexts)
            {
                var history = historyContext.History;
                var editItems = historyContext.EditItems;
                var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
                var values = editItems.Select(editItem => new object[] { history.Id, editItem.Id, editItem.Value });

                var insertValueQuery =
                    Sql.Insert(
                        costBlockMeta.HistoryMeta,
                        costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
                        history.Context.InputLevelId,
                        history.Context.CostElementId)
                       .Values(values);

                foreach (var relatedMeta in costBlockMeta.CoordinateFields)
                {
                    var historyRelatedMeta = costBlockMeta.HistoryMeta.GetRelatedMetaByName(relatedMeta.Name);
                    var insertQuery = Sql.Insert(
                        historyRelatedMeta,
                        historyRelatedMeta.CostBlockHistoryField.Name,
                        historyRelatedMeta.RelatedItemField.Name);

                    if (historyContext.RelatedItems.TryGetValue(relatedMeta.Name, out var relatedItemIds) && relatedItemIds.Length > 0)
                    {
                        var relatedItemsInsertValues = relatedItemIds.Select(relatedItemId => new object[] { history.Id, relatedItemId });

                        queries.Add(insertQuery.Values(relatedItemsInsertValues, relatedMeta.Name));
                    }
                    else
                    {
                        queries.Add(insertQuery.Values(relatedMeta.Name, history.Id, null));
                    }
                }

                queries.Add(insertValueQuery);
            }

            await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
        }

        public async Task<DataInfo<HistoryItemDto>> GetHistory(CostElementContext historyContext, IDictionary<string, long[]> filter, QueryInfo queryInfo = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);

            var historyEditUserIdColumnName = $"{nameof(CostBlockHistory.EditUser)}{nameof(User.Id)}";
            var historyEditUserIdColumnAlias = this.ToLowerFirstLetter(nameof(HistoryItemDto.EditUserId));
            var histroryEditUserIdColumn = new ColumnInfo(
                historyEditUserIdColumnName, 
                costBlockMeta.HistoryMeta.CostBlockHistoryField.ReferenceMeta.Name, 
                historyEditUserIdColumnAlias);

            var editDateColumnAlias = this.ToLowerFirstLetter(nameof(HistoryItemDto.EditDate));
            var editDateColumn = new ColumnInfo(
                nameof(CostBlockHistory.EditDate), 
                costBlockMeta.HistoryMeta.CostBlockHistoryField.ReferenceMeta.Name, 
                editDateColumnAlias);

            var userNameColumnAlias = this.ToLowerFirstLetter(nameof(HistoryItemDto.EditUserName));
            var userNameColumn = new ColumnInfo(nameof(User.Name), nameof(User), userNameColumnAlias);

            var selectColumns = new List<ColumnInfo>
            {
                editDateColumn,
                histroryEditUserIdColumn,
                userNameColumn
            };
            
            var selectQuery = this.historyQueryBuilder.BuildSelectHistoryValueQuery(historyContext, selectColumns);

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

            var userIdColumn = new ColumnInfo(nameof(User.Id), nameof(User));
            var options = new JoinHistoryValueQueryOptions
            {
                IsUseRegionCondition = true,
                InputLevelJoinType = InputLevelJoinType.All
            };

            var historyMeta = this.domainEnitiesMeta.CostBlockHistory;
            var whereCondition =
                ConditionHelper.AndStatic(filter.Convert(), costBlockMeta.Name)
                               .And(SqlOperators.Equals(historyMeta.ContextApplicationIdField.Name, historyContext.ApplicationId, historyMeta.Name))
                               .And(SqlOperators.Equals(historyMeta.ContextCostBlockIdField.Name, historyContext.CostBlockId, historyMeta.Name))
                               .And(SqlOperators.Equals(historyMeta.ContextCostElementIdField.Name, historyContext.CostElementId, historyMeta.Name));

            var historyQuery =
                this.historyQueryBuilder.BuildJoinHistoryValueQuery(historyContext, selectQuery, options)
                                        .Join(nameof(User), SqlOperators.Equals(histroryEditUserIdColumn, userIdColumn))
                                        .Where(whereCondition);

            var countHistoryQuery = Sql.Select(SqlFunctions.Count()).FromQuery(historyQuery, "t");

            var queryData = countHistoryQuery.ToQueryData();
            var count = await this.repositorySet.ExecuteScalarAsync<int>(queryData.Sql, queryData.Parameters);

            var historyItems = 
                await this.repositorySet.ReadBySqlAsync(
                    historyQuery.ByQueryInfo(queryInfo), 
                    reader => new HistoryItemDto
                    {
                        Value = reader.GetValue(0),
                        EditDate = reader.GetDateTime(1),
                        EditUserId = reader.GetInt64(2),
                        EditUserName = reader.GetString(3)
                    });

            return new DataInfo<HistoryItemDto>
            {
                Items = historyItems,
                Total = count
            };
        }

        private string ToLowerFirstLetter(string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }
    }
}
