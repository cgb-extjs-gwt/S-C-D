using System;
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
                queryData.JoinReferenceFields == null
                    ? new ReferenceFieldMeta[0]
                    : queryData.JoinReferenceFields.Select(queryData.CostBlock.GetField).Cast<ReferenceFieldMeta>().ToArray();

            var joinedColumns =
                joinFields.SelectMany(field => new[]
                          {
                              new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name),
                              new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name)
                          })
                          .ToArray();

            var valueField = queryData.CostBlock.CostElementsFields[queryData.CostElementId];
            var innerValueColumn = new ColumnInfo(valueField.Name);
            var valueApprovedField = queryData.CostBlock.CostElementsApprovedFields[valueField];
            var valueApprovedColumn = new ColumnInfo(valueApprovedField.Name);
            var innerApprovedColumn = BuildApprovedColumn();

            var maxValueColumn =
                queryData.CostBlock.CostElementsFields[queryData.CostElementId] is SimpleFieldMeta simpleField && 
                simpleField.Type == TypeCode.Boolean
                    ? SqlFunctions.Max(
                        SqlFunctions.Convert(new ColumnSqlBuilder(simpleField.Name), TypeCode.Int32))
                    : SqlFunctions.Max(queryData.CostElementId);

            var countColumn = SqlFunctions.Count(queryData.CostElementId, true);
            var approvedColumn = new ColumnInfo(innerApprovedColumn.Alias);
            var selectColumns = joinedColumns.Concat(new BaseColumnInfo[] { maxValueColumn, countColumn, approvedColumn }).ToArray();
            var groupByColumns = joinedColumns.Concat(new[] { approvedColumn }).ToArray();

            var selectQuery = Sql.Select(selectColumns);
            var query = queryData.IntoTable == null ? selectQuery : selectQuery.Into(queryData.IntoTable);

            return
                query.FromQuery(BuildInnerQuery(), queryData.Alias ?? "t")
                     .GroupBy(groupByColumns);

            QueryColumnInfo BuildApprovedColumn()
            {
                return new QueryColumnInfo(
                    new CaseSqlBuilder
                    {
                        Cases = new[]
                        {
                            new CaseItem
                            {
                                When = SqlOperators.Equals(innerValueColumn, valueApprovedColumn).ToSqlBuilder(),
                                Then = new RawSqlBuilder("1")
                            }
                        },
                        Else = new RawSqlBuilder("0")
                    },
                    "IsApproved");
            }

            GroupBySqlHelper BuildInnerQuery()
            {
                var columns = joinedColumns.Concat(new BaseColumnInfo[] { innerValueColumn, innerApprovedColumn }).ToArray();
                var joinInfos = joinFields.Select(field => new JoinInfo(queryData.CostBlock, field.Name));

                return
                    Sql.Select(columns)
                       .From(queryData.CostBlock)
                       .Join(joinInfos)
                       .WhereNotDeleted(queryData.CostBlock, queryData.Filter, queryData.CostBlock.Name);
            }
        }
    }
}
