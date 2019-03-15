using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockQueryBuilder : ICostBlockQueryBuilder
    {
        private readonly DomainEnitiesMeta meta;

        public CostBlockQueryBuilder(DomainEnitiesMeta meta)
        {
            this.meta = meta;
        }

        public OrderBySqlHelper BuildSelectQuery(CostBlockSelectQueryData queryData)
        {
            var costBlockAlias = queryData.Alias ?? "t";

            var joinFields = new List<ReferenceFieldMeta>();
            var selectGroupByColumns = queryData.GroupedFields.Select(fieldName => new ColumnInfo(fieldName)).ToList();

            if (queryData.IsGroupedFieldsNameSelected)
            {
                joinFields.AddRange(
                    queryData.GroupedFields.Select(queryData.CostBlock.GetField).OfType<ReferenceFieldMeta>());

                selectGroupByColumns.AddRange(
                    joinFields.Select(
                        field => 
                            new ColumnInfo(
                                field.ReferenceFaceField, 
                                field.ReferenceMeta.Name, 
                                $"{field.ReferenceMeta.Name}_{field.ReferenceFaceField}")));
            }

            var groupByColumns = selectGroupByColumns.Select(column => new ColumnInfo(column.Alias ?? column.Name)).ToArray();
            var selectColumns = groupByColumns.Concat(queryData.CostElementInfos.SelectMany(BuildSelectColumns)).ToArray();

            var selectQuery = Sql.Select(selectColumns);
            var query = queryData.IntoTable == null ? selectQuery : selectQuery.Into(queryData.IntoTable);

            return
                query.FromQuery(BuildInnerQuery(), costBlockAlias)
                     .GroupBy(groupByColumns);

            string BuildApprovedColumnAlias(string costElementId) => $"{costElementId}_IsApproved";

            QueryColumnInfo BuildApprovedColumn(FieldMeta valueField)
            {
                var valueApprovedField = queryData.CostBlock.CostElementsApprovedFields[valueField];
                var valueColumn = new ColumnInfo(valueField.Name);
                var valueApprovedColumn = new ColumnInfo(valueApprovedField.Name);
                var alias = BuildApprovedColumnAlias(valueField.Name);

                return new QueryColumnInfo(
                    new CaseSqlBuilder
                    {
                        Cases = new[]
                        {
                            new CaseItem
                            {
                                When = SqlOperators.Equals(valueColumn, valueApprovedColumn).ToSqlBuilder(),
                                Then = new RawSqlBuilder("1")
                            }
                        },
                        Else = new RawSqlBuilder("0")
                    },
                    alias);
            }

            GroupBySqlHelper BuildInnerQuery()
            {
                var joinInfos = joinFields.Select(field => new JoinInfo(queryData.CostBlock, field.Name));
                var columns =
                    queryData.CostElementInfos.Select(costElementInfo => queryData.CostBlock.CostElementsFields[costElementInfo.CostElementId])
                             .SelectMany(valueField => new BaseColumnInfo[]
                             {
                                 new ColumnInfo(valueField.Name),
                                 BuildApprovedColumn(valueField)
                             })
                             .Concat(selectGroupByColumns)
                             .ToArray();

                return
                    Sql.Select(columns)
                       .From(queryData.CostBlock)
                       .Join(joinInfos)
                       .WhereNotDeleted(queryData.CostBlock, queryData.Filter, queryData.CostBlock.Name);
            }

            IEnumerable<BaseColumnInfo> BuildSelectColumns(CostBlockSelectCostElementInfo costElementInfo)
            {
                var valueField = queryData.CostBlock.CostElementsFields[costElementInfo.CostElementId];

                var maxValueColumn =
                    valueField is SimpleFieldMeta simpleField &&
                    simpleField.Type == TypeCode.Boolean
                        ? SqlFunctions.Max(
                            SqlFunctions.Convert(
                                new ColumnSqlBuilder(simpleField.Name, costBlockAlias),
                                TypeCode.Int32),
                            costElementInfo.ValueColumnAlias)
                        : SqlFunctions.Max(valueField.Name, costBlockAlias, costElementInfo.ValueColumnAlias);

                var countColumn = SqlFunctions.Count(valueField.Name, true, costBlockAlias, costElementInfo.CountColumnAlias);
                var approvedColumnAlias = BuildApprovedColumnAlias(valueField.Name);
                var approvedColumn = SqlFunctions.Min(approvedColumnAlias, costBlockAlias, costElementInfo.IsApprovedColumnAlias);

                yield return maxValueColumn;
                yield return countColumn;
                yield return approvedColumn;
            }
        }
    }
}
