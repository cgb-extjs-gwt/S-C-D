//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Gdc.Scd.Core.Entities;
//using Gdc.Scd.Core.Meta.Constants;
//using Gdc.Scd.Core.Meta.Entities;
//using Gdc.Scd.DataAccessLayer.Entities;
//using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
//using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
//using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

//namespace Gdc.Scd.DataAccessLayer.Impl
//{
//    public class CostBlockValueHistoryQueryBuilder
//    {
//        private const string ValueColumnName = "value";

//        private readonly DomainEnitiesMeta domainEnitiesMeta;

//        private readonly DomainMeta domainMeta;

//        private HistoryContext historyContext;

//        private CostBlockEntityMeta costBlockMeta;

//        private CostBlockHistory history;

//        public CostBlockValueHistoryQueryBuilder(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
//        {
//            this.domainEnitiesMeta = domainEnitiesMeta;
//            this.domainMeta = domainMeta;
//        }

//        public CostBlockValueHistoryQueryBuilder SetHistoryContext(HistoryContext historyContext)
//        {
//            this.historyContext = historyContext;
//            this.costBlockMeta = (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(historyContext.CostBlockId, historyContext.ApplicationId);

//            return this;
//        }

//        public CostBlockValueHistoryQueryBuilder SetHistory(CostBlockHistory history)
//        {
//            var builder = this.SetHistoryContext(history.Context);

//            builder.history = history;

//            return builder;
//        }

//        public SelectJoinSqlHelper SelectHistoryValue(IEnumerable<BaseColumnInfo> addingSelectColumns)
//        {
//            var inputLevelAlias = GetAlias(this.costBlockMeta.HistoryMeta);

//            ColumnInfo costElementColumn;

//            var costElementField = this.costBlockMeta.CostElementsFields[this.historyContext.CostElementId];
//            var referenceCostElementField = costElementField as ReferenceFieldMeta;
//            if (referenceCostElementField == null)
//            {
//                costElementColumn = new ColumnInfo(costElementField.Name, this.costBlockMeta.HistoryMeta.Name, ValueColumnName);
//            }
//            else
//            {
//                costElementColumn = new ColumnInfo(referenceCostElementField.ReferenceFaceField, referenceCostElementField.ReferenceMeta.Name, ValueColumnName);
//            }

//            var selectQuery =
//                Sql.SelectDistinct(new[] { costElementColumn }.Concat(addingSelectColumns).ToArray())
//                   .From(this.costBlockMeta.HistoryMeta);

//            if (referenceCostElementField != null)
//            {
//                selectQuery = selectQuery.Join(this.costBlockMeta.HistoryMeta, referenceCostElementField.Name);
//            }

//            return selectQuery;
//        }

//        public SqlHelper JoinApproveHistoryValue<TQuery>(TQuery query, IEnumerable<JoinInfo> joinInfos = null)
//            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
//        {
//            var costBlockJoinCondition = BuildHistoryInputLevelJoinCondition(this.costBlockMeta, this.history.Context);

//            query = this.BuildJoinHistoryValueQuery(query, costBlockJoinCondition.ToSqlBuilder());

//            if (joinInfos != null)
//            {
//                foreach (var joinInfo in joinInfos)
//                {
//                    query = query.Join(joinInfo.Meta, joinInfo.ReferenceFieldName, joinInfo.Alias);
//                }
//            }

//            var historyIdCondition =
//                SqlOperators.Equals(
//                    costBlockMeta.HistoryMeta.CostBlockHistoryField.Name,
//                    "costBlockHistoryId",
//                    this.history.Id,
//                    costBlockMeta.HistoryMeta.Name);

//            return query.Where(historyIdCondition);
//        }

//        private TQuery BuildJoinHistoryValueQuery<TQuery>(TQuery query, ISqlBuilder costBlockJoinAdditionalCondition = null)
//            where TQuery : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<TQuery>
//        {
//            var createdDateCondition =
//                SqlOperators.LessOrEqual(
//                    new ColumnInfo(this.costBlockMeta.CreatedDateField.Name, costBlockMeta.Name),
//                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName));

//            var deletedDateCondition = ConditionHelper.OrBrackets(
//                SqlOperators.IsNull(this.costBlockMeta.DeletedDateField.Name, this.costBlockMeta.Name),
//                SqlOperators.GreaterOrEqual(new ColumnInfo(this.costBlockMeta.DeletedDateField.Name, this.costBlockMeta.Name),
//                    new ColumnInfo(nameof(CostBlockHistory.EditDate), MetaConstants.CostBlockHistoryTableName)));

//            var costBlockJoinCondition = ConditionHelper.And(createdDateCondition, deletedDateCondition);

//            if (this.historyContext.RegionInputId.HasValue)
//            {
//                var region = this.domainMeta.CostBlocks[this.historyContext.CostBlockId].CostElements[this.historyContext.CostElementId].RegionInput;
//                var historyRegionIdName = $"{nameof(CostBlockHistory.Context)}_{nameof(HistoryContext.RegionInputId)}";
//                var regionCondition = SqlOperators.Equals(
//                        new ColumnInfo(historyRegionIdName, nameof(CostBlockHistory)),
//                        new ColumnInfo(region.Id, this.costBlockMeta.Name));

//                costBlockJoinCondition = costBlockJoinCondition.And(regionCondition);
//            }

//            if (costBlockJoinAdditionalCondition != null)
//            {
//                costBlockJoinCondition = costBlockJoinCondition.AndBrackets(costBlockJoinAdditionalCondition);
//            }

//            foreach (var relatedMeta in this.costBlockMeta.HistoryMeta.RelatedMetas)
//            {
//                if (relatedMeta.Name != this.historyContext.InputLevelId)
//                {
//                    var joinCondition = SqlOperators.Equals(
//                        new ColumnInfo(this.costBlockMeta.HistoryMeta.CostBlockHistoryField.Name, this.costBlockMeta.HistoryMeta.Name),
//                        new ColumnInfo(relatedMeta.CostBlockHistoryField.Name, relatedMeta.Name));

//                    query = query.Join(relatedMeta, joinCondition);

//                    var isNullCondition = SqlOperators.IsNull(relatedMeta.RelatedItemField.Name, relatedMeta.Name);
//                    var equalCondition = SqlOperators.Equals(
//                        new ColumnInfo(relatedMeta.RelatedItemField.Name, relatedMeta.Name),
//                        new ColumnInfo(relatedMeta.RelatedItemField.Name, this.costBlockMeta.Name));

//                    costBlockJoinCondition = costBlockJoinCondition.And(
//                        ConditionHelper.OrBrackets(isNullCondition, equalCondition));
//                }
//            }

//            return
//                query.Join(this.costBlockMeta.HistoryMeta, costBlockMeta.HistoryMeta.CostBlockHistoryField.Name)
//                     .Join(this.costBlockMeta, costBlockJoinCondition);
//        }

//        public static string GetAlias(BaseEntityMeta meta)
//        {
//            return $"{meta.Schema}_{meta.Name}";
//        }

//        public ConditionHelper BuildHistoryInputLevelJoinCondition(CostBlockEntityMeta costBlockMeta, HistoryContext historyContext)
//        {
//            return SqlOperators.Equals(
//                new ColumnInfo(historyContext.InputLevelId, costBlockMeta.HistoryMeta.Name),
//                new ColumnInfo(historyContext.InputLevelId, costBlockMeta.Name));
//        }
//    }
//}
