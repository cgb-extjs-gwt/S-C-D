using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryRepository : ICostBlockValueHistoryRepository
    {
        private readonly DomainEnitiesMeta domainEnitiesMeta;
        private readonly IRepositorySet repositorySet;

        public CostBlockValueHistoryRepository(DomainEnitiesMeta domainEnitiesMeta, IRepositorySet repositorySet)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.repositorySet = repositorySet;
        }

        public async Task Save(CostBlockHistory history, IEnumerable<EditItem> editItems, IDictionary<string, long[]> relatedItems)
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(history);
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
                    var relatedItemsInsertValues = relatedItemIds.Select(relatedItemId => new[] { history.Id, relatedItemId });

                    insertRelatedItemsQueries.Add(insertQuery.Values(relatedMeta.Name, relatedItemsInsertValues));
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
            var costBlockMeta = this.GetCostBlockEntityMeta(history);
            var inputLevelField = costBlockMeta.InputLevelFields[history.Context.InputLevelId];
            var inputLevelMeta = (NamedEntityMeta)inputLevelField.ReferenceMeta;

            var costElementColumn = new ColumnInfo(history.Context.CostElementId, costBlockMeta.HistoryMeta.Name);
            var inputLevelIdColumn = new ColumnInfo(inputLevelMeta.IdField.Name, costBlockMeta.HistoryMeta.Name, "InputLevelId");
            var inputLevelAlias = this.GetAlias(costBlockMeta.HistoryMeta);
            var inputLevelNameColumn = new ColumnInfo(inputLevelMeta.NameField.Name, inputLevelAlias, "InputLevelName");

            var selectColumns = new List<ColumnInfo> { costElementColumn, inputLevelIdColumn, inputLevelNameColumn };

            foreach (var dependecyField in costBlockMeta.DependencyFields)
            {
                selectColumns.Add(new ColumnInfo(dependecyField.Name, costBlockMeta.Name));
                selectColumns.Add(new ColumnInfo(dependecyField.ReferenceFaceField, this.GetAlias(dependecyField.ReferenceMeta), $"{dependecyField.Name}_Name"));
            }

            var selectQuery =
                Sql.SelectDistinct(selectColumns.ToArray())
                   .From(costBlockMeta.HistoryMeta)
                   .Join(costBlockMeta.HistoryMeta, history.Context.InputLevelId, inputLevelAlias);

            var costBlockJoinCondition = SqlOperators.Equals(
                    new ColumnInfo(history.Context.InputLevelId, costBlockMeta.HistoryMeta.Name),
                    new ColumnInfo(history.Context.InputLevelId, costBlockMeta.Name));

            foreach (var relatedMeta in costBlockMeta.HistoryMeta.RelatedMetas)
            {
                selectQuery = selectQuery.Join(
                    relatedMeta, 
                    SqlOperators.Equals(
                        new ColumnInfo(costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, costBlockMeta.HistoryMeta.Name), 
                        new ColumnInfo(relatedMeta.CostBlockHistoryField.Name, relatedMeta.Name)));

                var isNullCondition = SqlOperators.IsNull(relatedMeta.RelatedItemField.Name, relatedMeta.Name);
                var equalCondition = SqlOperators.Equals(
                    new ColumnInfo(relatedMeta.RelatedItemField.Name, relatedMeta.Name), 
                    new ColumnInfo(relatedMeta.RelatedItemField.Name, costBlockMeta.Name));

                costBlockJoinCondition = costBlockJoinCondition.And(
                    ConditionHelper.OrBrackets(isNullCondition, equalCondition));
            }

            selectQuery = selectQuery.Join(costBlockMeta, costBlockJoinCondition);

            foreach (var dependecyField in costBlockMeta.DependencyFields)
            {
                selectQuery = selectQuery.Join(costBlockMeta, dependecyField.Name, this.GetAlias(dependecyField.ReferenceMeta));
            }

            var historyIdCondition = 
                SqlOperators.Equals(
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, 
                    "costBlockHistoryId", 
                    history.Id, 
                    costBlockMeta.HistoryMeta.Name);

            var createdDateCondition = SqlOperators.LessOrEqual(costBlockMeta.CreatedDateField.Name, "editDate", history.EditDate, costBlockMeta.Name);
            var deletedDateCondition = ConditionHelper.OrBrackets(
                SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                SqlOperators.GreaterOrEqual(costBlockMeta.DeletedDateField.Name, "editDate", history.EditDate, costBlockMeta.Name));

            var query =
                selectQuery.Where(ConditionHelper.And(createdDateCondition, deletedDateCondition).And(historyIdCondition));

            return await this.repositorySet.ReadBySql(query, this.CostBlockValueHistoryMap);
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

        private CostBlockEntityMeta GetCostBlockEntityMeta(CostBlockHistory history)
        {
            return (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(history.Context.CostBlockId, history.Context.ApplicationId);
        }
    }
}
