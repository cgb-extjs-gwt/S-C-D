using System.Collections.Generic;
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
        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly DomainMeta domainMeta;

        public CostBlockValueHistoryQueryBuilder(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.domainMeta = domainMeta;
        }

        public SelectJoinSqlHelper BuildSelectHistoryValueQuery(CostElementContext historyContext, IEnumerable<BaseColumnInfo> addingSelectColumns = null)
        {
            const string ValueColumnName = "value";

            var costBlockMeta = this.domainEnitiesMeta.CostBlocks[historyContext];

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

            var columns = new List<BaseColumnInfo>
            {
                costElementColumn
            };

            if (addingSelectColumns != null)
            {
                columns.AddRange(addingSelectColumns);
            }

            var selectQuery = Sql.SelectDistinct(columns.ToArray()).From(costBlockMeta.HistoryMeta);

            if (referenceCostElementField != null)
            {
                selectQuery = selectQuery.Join(costBlockMeta.HistoryMeta, referenceCostElementField.Name);
            }

            return selectQuery;
        }

        public TQuery BuildJoinHistoryValueQuery<TQuery>(CostElementContext historyContext, TQuery query, JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            if (options == null)
            {
                options = new JoinHistoryValueQueryOptions();
            }

            var costBlockMeta = this.domainEnitiesMeta.CostBlocks[historyContext];
            var costBlockAllVersionsAlias = options.UseActualVersionRows ? $"{costBlockMeta.Name}_AllVersions" : costBlockMeta.Name;
            var costBlockJoinCondition = BuildCostBlockJoinCondition();

            foreach (var relatedMeta in costBlockMeta.HistoryMeta.RelatedMetas)
            {
                if (relatedMeta.Name != historyContext.InputLevelId)
                {
                    var joinCondition = SqlOperators.Equals(
                        new ColumnInfo(costBlockMeta.HistoryMeta.CostBlockHistoryField, costBlockMeta.HistoryMeta),
                        new ColumnInfo(relatedMeta.CostBlockHistoryField, relatedMeta));

                    query = query.Join(relatedMeta, joinCondition);

                    var isNullCondition = SqlOperators.IsNull(relatedMeta.RelatedItemField.Name, relatedMeta.Name);
                    var equalCondition = SqlOperators.Equals(
                        new ColumnInfo(relatedMeta.RelatedItemField, relatedMeta),
                        new ColumnInfo(relatedMeta.RelatedItemField, costBlockAllVersionsAlias));

                    costBlockJoinCondition = costBlockJoinCondition.And(
                        ConditionHelper.OrBrackets(isNullCondition, equalCondition));
                }
            }

            query =
                query.Join(costBlockMeta.HistoryMeta, costBlockMeta.HistoryMeta.CostBlockHistoryField.Name)
                     .Join(costBlockMeta, costBlockJoinCondition, JoinType.Inner, costBlockAllVersionsAlias)
                     .Join(options.JoinInfos);

            if (options.UseActualVersionRows)
            {
                query = query.Join(costBlockMeta, BuildCostBlockActualCoordinates());
            }

            return query;

            ConditionHelper BuildCostBlockJoinCondition()
            {
                var costBlockHistoryMeta = this.domainEnitiesMeta.CostBlockHistory;
                var createdDateCondition =
                    SqlOperators.LessOrEqual(
                        new ColumnInfo(costBlockMeta.CreatedDateField.Name, costBlockAllVersionsAlias),
                        new ColumnInfo(costBlockHistoryMeta.EditDateField, costBlockHistoryMeta));

                var deletedDateCondition = SqlOperators.IsNull(costBlockMeta.DeletedDateField.Name, costBlockAllVersionsAlias);

                if (options.CostBlockFilter != null)
                {
                    deletedDateCondition = ConditionHelper.AndBrackets(
                        deletedDateCondition,
                        ConditionHelper.AndStatic(options.CostBlockFilter, costBlockAllVersionsAlias));
                }

                if (options.UseActualVersionRows)
                {
                    deletedDateCondition = deletedDateCondition.OrBrackets(
                        ConditionHelper.And(
                            SqlOperators.IsNotNull(costBlockMeta.ActualVersionField.Name, costBlockAllVersionsAlias),
                            SqlOperators.IsNotNull(costBlockMeta.DeletedDateField.Name, costBlockAllVersionsAlias)));
                }

                var condition = createdDateCondition.AndBrackets(deletedDateCondition).And(BuildOptionCostBlockJoinCondition());

                return condition;

                ConditionHelper BuildOptionCostBlockJoinCondition()
                {
                    var conditions = new List<ConditionHelper>();

                    if (options.UseRegionCondition && historyContext.RegionInputId.HasValue)
                    {
                        var costElement = this.domainMeta.GetCostElement(historyContext);
                        var regionCondition = SqlOperators.Equals(
                            new ColumnInfo(costElement.RegionInput.Id, costBlockAllVersionsAlias),
                            new ColumnInfo(costBlockHistoryMeta.ContextRegionInputIdField, costBlockHistoryMeta));

                        conditions.Add(regionCondition);
                    }

                    if (options.InputLevelJoinType.HasValue)
                    {
                        switch (options.InputLevelJoinType)
                        {
                            case InputLevelJoinType.HistoryContext:
                                var inputLevelCondition = SqlOperators.Equals(
                                    new ColumnInfo(historyContext.InputLevelId, costBlockMeta.HistoryMeta.Name),
                                    new ColumnInfo(historyContext.InputLevelId, costBlockAllVersionsAlias));

                                conditions.Add(inputLevelCondition);
                                break;

                            case InputLevelJoinType.All:
                                var costElementMeta = this.domainMeta.GetCostElement(historyContext);
                                var inputLevelConditions = new List<ConditionHelper>();

                                foreach (var inputLevel in costElementMeta.InputLevels)
                                {
                                    inputLevelConditions.Add(
                                        SqlOperators.Equals(
                                            new ColumnInfo(inputLevel.Id, costBlockMeta.HistoryMeta.Name),
                                            new ColumnInfo(inputLevel.Id, costBlockAllVersionsAlias)));
                                }

                                conditions.Add(ConditionHelper.OrBrackets(inputLevelConditions));
                                break;
                        }
                    }

                    return ConditionHelper.And(conditions);
                }
            }

            ConditionHelper BuildCostBlockActualCoordinates()
            {
                var condition =
                    ConditionHelper.OrBrackets(
                        SqlOperators.Equals(
                            new ColumnInfo(costBlockMeta.ActualVersionField, costBlockAllVersionsAlias),
                            new ColumnInfo(costBlockMeta.IdField, costBlockMeta)),
                        SqlOperators.Equals(
                            new ColumnInfo(costBlockMeta.IdField, costBlockAllVersionsAlias),
                            new ColumnInfo(costBlockMeta.IdField, costBlockMeta)));

                if (options.CostBlockFilter != null)
                {
                    condition = condition.And(options.CostBlockFilter, costBlockMeta.Name);
                }

                return condition;
            }
        }

        public SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(
            CostBlockHistory history,
            TQuery query,
            InputLevelJoinType inputLevelJoinType = InputLevelJoinType.HistoryContext,
            IEnumerable<JoinInfo> joinInfos = null,
            long? historyValueId = null,
            IDictionary<string, IEnumerable<object>> costBlockFiter = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var options = new JoinHistoryValueQueryOptions
            {
                UseRegionCondition = true,
                InputLevelJoinType = inputLevelJoinType,
                JoinInfos = joinInfos
            };

            query = this.BuildJoinHistoryValueQuery(history.Context, query, options);

            var costBlockMeta = this.domainEnitiesMeta.CostBlocks[history.Context];
            var whereCondition =
                SqlOperators.Equals(
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
                    history.Id,
                    costBlockMeta.HistoryMeta.Name);

            if (historyValueId.HasValue)
            {
                whereCondition = whereCondition.And(SqlOperators.Equals(
                    MetaConstants.IdFieldKey,
                    historyValueId,
                    costBlockMeta.HistoryMeta.Name));
            }

            if (costBlockFiter != null && costBlockFiter.Count > 0)
            {
                whereCondition = whereCondition.AndBrackets(
                    ConditionHelper.AndStatic(costBlockFiter, costBlockMeta.Name));
            }

            return query.Where(whereCondition);
        }
    }
}
