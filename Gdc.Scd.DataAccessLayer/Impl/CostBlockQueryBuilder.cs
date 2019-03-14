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
            var joinFields =
                queryData.JoinedReferenceFields == null
                    ? new ReferenceFieldMeta[0]
                    : queryData.JoinedReferenceFields.Select(queryData.CostBlock.GetField).Cast<ReferenceFieldMeta>().ToArray();

            var joinedColumns =
                joinFields.SelectMany(field => new[]
                          {
                              new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name),
                              new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name)
                          })
                          .ToArray();

            var selectColumns = joinedColumns.Concat(queryData.CostElementInfos.SelectMany(BuildSelectColumns)).ToArray();
            var groupByColumns = queryData.GroupedFields.Select(fieldName => new ColumnInfo(fieldName)).Concat(joinedColumns).ToArray();

            var selectQuery = Sql.Select(selectColumns);
            var query = queryData.IntoTable == null ? selectQuery : selectQuery.Into(queryData.IntoTable);

            return
                query.FromQuery(BuildInnerQuery(), queryData.Alias ?? "t")
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
                             .Concat(joinedColumns)
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
                            SqlFunctions.Convert(new ColumnSqlBuilder(simpleField.Name), TypeCode.Int32), costElementInfo.ValueColumnAlias)
                        : SqlFunctions.Max(valueField.Name, costElementInfo.ValueColumnAlias);

                var countColumn = SqlFunctions.Count(valueField.Name, true, costElementInfo.CountColumnAlias);
                var approvedColumnAlias = BuildApprovedColumnAlias(valueField.Name);
                var approvedColumn = SqlFunctions.Min(approvedColumnAlias, costElementInfo.IsApprovedColumnAlias);

                yield return maxValueColumn;
                yield return countColumn;
                yield return approvedColumn;
            }
        }
    }
}
