using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryQueryBuilder : ICostBlockValueHistoryQueryBuilder
    {
        private const string ValueColumnName = "value";

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly DomainMeta domainMeta;

        public CostBlockValueHistoryQueryBuilder(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.domainMeta = domainMeta;
        }

        public SelectJoinSqlHelper BuildSelectHistoryValueQuery(HistoryContext historyContext, IEnumerable<BaseColumnInfo> addingSelectColumns)
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

        public TQuery BuildJoinHistoryValueQuery<TQuery>(HistoryContext historyContext, TQuery query, JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var costBlockMeta = this.GetCostBlockEntityMeta(historyContext);

            var createdDateCondition =
                SqlOperators.LessOrEqual(
                    new ColumnInfo(costBlockMeta.CreatedDateField.Name, costBlockMeta.Name),
                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName));

            var deletedDateCondition = ConditionHelper.OrBrackets(
                SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                SqlOperators.GreaterOrEqual(new ColumnInfo(costBlockMeta.DeletedDateField.Name, costBlockMeta.Name),
                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName)));

            var costBlockJoinCondition = ConditionHelper.And(createdDateCondition, deletedDateCondition);

            if (options != null)
            {
                costBlockJoinCondition.And(this.BuildCostBlockJoinCondition(historyContext, costBlockMeta, options));
            }

            foreach (var relatedMeta in costBlockMeta.HistoryMeta.RelatedMetas)
            {
                if (relatedMeta.Name != historyContext.InputLevelId)
                {
                    var joinCondition = SqlOperators.Equals(
                        new ColumnInfo(costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, costBlockMeta.HistoryMeta.Name),
                        new ColumnInfo(relatedMeta.CostBlockHistoryField.Name, relatedMeta.Name));

                    query = query.Join(relatedMeta, joinCondition);

                    var isNullCondition = SqlOperators.IsNull(relatedMeta.RelatedItemField.Name, relatedMeta.Name);
                    var equalCondition = SqlOperators.Equals(
                        new ColumnInfo(relatedMeta.RelatedItemField.Name, relatedMeta.Name),
                        new ColumnInfo(relatedMeta.RelatedItemField.Name, costBlockMeta.Name));

                    costBlockJoinCondition = costBlockJoinCondition.And(
                        ConditionHelper.OrBrackets(isNullCondition, equalCondition));
                }
            }

            query =
                query.Join(costBlockMeta.HistoryMeta, costBlockMeta.HistoryMeta.CostBlockHistoryField.Name)
                     .Join(costBlockMeta, costBlockJoinCondition);

            if (options != null)
            {
                query = query.Join(options.JoinInfos);
            }

            return query;
        }

        public SqlHelper BuildJoinHistoryValueQuery<TQuery>(CostBlockHistory history, TQuery query, JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            query = this.BuildJoinHistoryValueQuery(history.Context, query, options);

            var costBlockMeta = this.GetCostBlockEntityMeta(history.Context);
            var historyIdCondition =
                SqlOperators.Equals(
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
                    "costBlockHistoryId",
                    history.Id,
                    costBlockMeta.HistoryMeta.Name);

            return query.Where(historyIdCondition);
        }

        public SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(CostBlockHistory history, TQuery query, IEnumerable<JoinInfo> joinInfos = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var options = new JoinHistoryValueQueryOptions
            {
                IsUseRegionCondition = true,
                InputLevelJoinType = InputLevelJoinType.HisotoryContext,
                JoinInfos = joinInfos
            };

            return this.BuildJoinHistoryValueQuery(history, query);
        }

        public CostBlockEntityMeta GetCostBlockEntityMeta(HistoryContext historyContext)
        {
            return (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(historyContext.CostBlockId, historyContext.ApplicationId);
        }

        public string GetAlias(BaseEntityMeta meta)
        {
            return $"{meta.Schema}_{meta.Name}";
        }

        private ConditionHelper BuildCostBlockJoinCondition(HistoryContext historyContext, CostBlockEntityMeta costBlockMeta, JoinHistoryValueQueryOptions options)
        {
            var conditions = new List<ConditionHelper>();

            if (options.IsUseRegionCondition && historyContext.RegionInputId.HasValue)
            {
                var region = this.domainMeta.CostBlocks[historyContext.CostBlockId].CostElements[historyContext.CostElementId].RegionInput;
                var historyRegionIdName = $"{nameof(CostBlockHistory.Context)}_{nameof(HistoryContext.RegionInputId)}";
                var regionCondition = SqlOperators.Equals(
                        new ColumnInfo(historyRegionIdName, nameof(CostBlockHistory)),
                        new ColumnInfo(region.Id, costBlockMeta.Name));

                conditions.Add(regionCondition);
            }

            switch (options.InputLevelJoinType)
            {
                case InputLevelJoinType.HisotoryContext:
                    var inputLevelCondition = SqlOperators.Equals(
                        new ColumnInfo(historyContext.InputLevelId, costBlockMeta.HistoryMeta.Name),
                        new ColumnInfo(historyContext.InputLevelId, costBlockMeta.Name));

                    conditions.Add(inputLevelCondition);
                    break;

                case InputLevelJoinType.All:
                    var costElementMeta = this.domainMeta.CostBlocks[historyContext.CostBlockId].CostElements[historyContext.CostElementId];
                    var inputLevelConditions = new List<ConditionHelper>();

                    foreach (var inputLevel in costElementMeta.InputLevels)
                    {
                        inputLevelConditions.Add(
                            SqlOperators.Equals(
                                new ColumnInfo(inputLevel.Id, costBlockMeta.HistoryMeta.Name),
                                new ColumnInfo(inputLevel.Id, costBlockMeta.Name)));
                    }

                    conditions.Add(ConditionHelper.OrBrackets(inputLevelConditions));
                    break;
            }

            return ConditionHelper.And(conditions);
        }
    }
}
