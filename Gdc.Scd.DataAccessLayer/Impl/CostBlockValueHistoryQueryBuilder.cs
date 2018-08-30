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
        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly DomainMeta domainMeta;

        public CostBlockValueHistoryQueryBuilder(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
        {
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.domainMeta = domainMeta;
        }

        public SelectJoinSqlHelper BuildSelectHistoryValueQuery(
            HistoryContext historyContext, 
            IEnumerable<BaseColumnInfo> addingSelectColumns = null, 
            string valueColumnName = "value")
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var inputLevelAlias = this.GetAlias(costBlockMeta.HistoryMeta);

            ColumnInfo costElementColumn;

            var costElementField = costBlockMeta.CostElementsFields[historyContext.CostElementId];
            var referenceCostElementField = costElementField as ReferenceFieldMeta;
            if (referenceCostElementField == null)
            {
                costElementColumn = new ColumnInfo(costElementField.Name, costBlockMeta.HistoryMeta.Name, valueColumnName);
            }
            else
            {
                costElementColumn = new ColumnInfo(referenceCostElementField.ReferenceFaceField, referenceCostElementField.ReferenceMeta.Name, valueColumnName);
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

        public SqlHelper BuildSelectJoinHistoryValueQuery(
            CostBlockHistory history,
            string inputLevelId,
            InputLevelJoinType inputLevelJoinType,
            long? historyValueId = null,
            string valueColumnName = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var inputLevelField = costBlockMeta.InputLevelFields[inputLevelId];
            var inputLevelMeta = (NamedEntityMeta)inputLevelField.ReferenceMeta;

            var inputLevelIdColumn = new ColumnInfo(inputLevelMeta.IdField.Name, costBlockMeta.HistoryMeta.Name, "InputLevelId");
            var inputLevelAlias = this.GetAlias(costBlockMeta.HistoryMeta);
            var inputLevelNameColumn = new ColumnInfo(inputLevelMeta.NameField.Name, inputLevelAlias, "InputLevelName");

            var selectColumns = new List<ColumnInfo> { inputLevelIdColumn, inputLevelNameColumn };
            selectColumns.AddRange(this.GetDependencyColumns(costBlockMeta));
            selectColumns.Add(new ColumnInfo(MetaConstants.IdFieldKey, costBlockMeta.HistoryMeta.Name, "HistoryValueId"));

            var selectQuery = this.BuildSelectHistoryValueQuery(history.Context, selectColumns, valueColumnName);
            var joinInfos = this.GetDependencyJoinInfos(costBlockMeta).ToList();

            joinInfos.Add(new JoinInfo
            {
                Meta = costBlockMeta.HistoryMeta,
                ReferenceFieldName = inputLevelField.Name,
                JoinedTableAlias = inputLevelAlias
            });

            return this.BuildJoinApproveHistoryValueQuery(history, selectQuery, inputLevelJoinType, joinInfos, historyValueId);
        }

        public TQuery BuildJoinHistoryValueQuery<TQuery>(HistoryContext historyContext, TQuery query, JoinHistoryValueQueryOptions options = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);

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
                costBlockJoinCondition = 
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

        public SqlHelper BuildJoinHistoryValueQuery<TQuery>(
            CostBlockHistory history, 
            TQuery query, 
            JoinHistoryValueQueryOptions options = null, 
            long? historyValueId = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            query = this.BuildJoinHistoryValueQuery(history.Context, query, options);

            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(history.Context);
            var whereCondition =
                SqlOperators.Equals(
                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
                    "costBlockHistoryId",
                    history.Id,
                    costBlockMeta.HistoryMeta.Name);

            if (historyValueId.HasValue)
            {
                whereCondition = whereCondition.And(SqlOperators.Equals(
                    MetaConstants.IdFieldKey, 
                    "historyValueId",
                    historyValueId,
                    costBlockMeta.HistoryMeta.Name));
            }

            return query.Where(whereCondition);
        }

        public SqlHelper BuildJoinApproveHistoryValueQuery<TQuery>(
            CostBlockHistory history, 
            TQuery query, 
            InputLevelJoinType inputLevelJoinType = InputLevelJoinType.HistoryContext, 
            IEnumerable<JoinInfo> joinInfos = null,
            long? historyValueId = null)
            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
        {
            var options = new JoinHistoryValueQueryOptions
            {
                IsUseRegionCondition = true,
                InputLevelJoinType = inputLevelJoinType,
                JoinInfos = joinInfos
            };

            return this.BuildJoinHistoryValueQuery(history, query, options, historyValueId);
        }

        private string GetAlias(BaseEntityMeta meta)
        {
            return $"{meta.Schema}_{meta.Name}";
        }

        private ConditionHelper BuildCostBlockJoinCondition(HistoryContext historyContext, CostBlockEntityMeta costBlockMeta, JoinHistoryValueQueryOptions options)
        {
            var conditions = new List<ConditionHelper>();

            if (options.IsUseRegionCondition && historyContext.RegionInputId.HasValue)
            {
                var region = this.domainMeta.GetCostElement(historyContext).RegionInput;
                var historyRegionIdName = $"{nameof(CostBlockHistory.Context)}_{nameof(HistoryContext.RegionInputId)}";
                var regionCondition = SqlOperators.Equals(
                        new ColumnInfo(historyRegionIdName, nameof(CostBlockHistory)),
                        new ColumnInfo(region.Id, costBlockMeta.Name));

                conditions.Add(regionCondition);
            }

            switch (options.InputLevelJoinType)
            {
                case InputLevelJoinType.HistoryContext:
                    var inputLevelCondition = SqlOperators.Equals(
                        new ColumnInfo(historyContext.InputLevelId, costBlockMeta.HistoryMeta.Name),
                        new ColumnInfo(historyContext.InputLevelId, costBlockMeta.Name));

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
                                new ColumnInfo(inputLevel.Id, costBlockMeta.Name)));
                    }

                    conditions.Add(ConditionHelper.OrBrackets(inputLevelConditions));
                    break;
            }

            return ConditionHelper.And(conditions);
        }

        private IEnumerable<ColumnInfo> GetDependencyColumns(CostBlockEntityMeta costBlockMeta)
        {
            foreach (var dependecyField in costBlockMeta.DependencyFields)
            {
                yield return new ColumnInfo(dependecyField.Name, costBlockMeta.Name);
                yield return new ColumnInfo(
                    dependecyField.ReferenceFaceField,
                    this.GetAlias(dependecyField.ReferenceMeta),
                    $"{dependecyField.Name}_Name");
            }
        }

        private IEnumerable<JoinInfo> GetDependencyJoinInfos(CostBlockEntityMeta costBlockMeta)
        {
            return costBlockMeta.DependencyFields.Select(field => new JoinInfo
            {
                Meta = costBlockMeta,
                ReferenceFieldName = field.Name,
                JoinedTableAlias = this.GetAlias(field.ReferenceMeta)
            });
        }
    }
}
