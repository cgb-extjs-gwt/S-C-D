using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostEditorRepository : ICostEditorRepository
    {
        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public CostEditorRepository(IRepositorySet repositorySet, DomainEnitiesMeta domainEnitiesMeta)
        {
            this.repositorySet = repositorySet;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context, IDictionary<string, long[]> filter)
        {
            return await this.GetEditItems(context, filter.Convert());
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(context);

            var nameField = costBlockMeta.InputLevelFields[context.InputLevelId];
            var innerNameColumn = new ColumnInfo(nameField.ReferenceFaceField, nameField.ReferenceMeta.Name);
            var innerNameIdColumn = new ColumnInfo(nameField.ReferenceValueField, nameField.ReferenceMeta.Name);

            var valueField = costBlockMeta.CostElementsFields[context.CostElementId];
            var innerValueColumn = new ColumnInfo(valueField.Name);
            var valueApprovedField = costBlockMeta.CostElementsApprovedFields[valueField];
            var valueApprovedColumn = new ColumnInfo(valueApprovedField.Name);

            var innerApprovedColumn = new QueryColumnInfo(
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

            var innerQuery =
                Sql.Select(innerNameIdColumn, innerNameColumn, innerValueColumn, innerApprovedColumn)
                   .From(costBlockMeta)
                   .Join(costBlockMeta, nameField.Name)
                   .WhereNotDeleted(costBlockMeta, filter, costBlockMeta.Name);

            var nameIdCoumn = new ColumnInfo(innerNameIdColumn.Name);
            var nameColumn = new ColumnInfo(innerNameColumn.Name);

            var maxValueColumn =
                costBlockMeta.CostElementsFields[context.CostElementId] is SimpleFieldMeta simpleField && simpleField.Type == TypeCode.Boolean
                    ? SqlFunctions.Max(
                        SqlFunctions.Convert(new ColumnSqlBuilder(simpleField.Name), TypeCode.Int32))
                    : SqlFunctions.Max(context.CostElementId);

            var countColumn = SqlFunctions.Count(context.CostElementId, true);
            var approvedColumn = new ColumnInfo(innerApprovedColumn.Alias);

            var query =
                Sql.Select(nameIdCoumn, nameColumn, maxValueColumn, countColumn, approvedColumn)
                   .FromQuery(innerQuery, "t")
                   .GroupBy(nameIdCoumn, nameColumn, approvedColumn)
                   .OrderBy(new OrderByInfo
                   {
                       Direction = SortDirection.Asc,
                       SqlBuilder = new ColumnSqlBuilder(nameColumn.Name)
                   });

            return await this.repositorySet.ReadBySql(query, reader =>
            {
                var valueCount = reader.GetInt32(3);

                return new EditItem
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Value = valueCount == 1 ? reader.GetValue(2) : null,
                    ValueCount = valueCount,
                    IsApproved = reader.GetInt32(4) == 1
                };
            });
        }
    }
}
