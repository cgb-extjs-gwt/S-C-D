using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

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

        public async Task<IEnumerable<EditItem>> GetEditItems(EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetEntityMeta(editItemInfo.EntityName, editItemInfo.Schema);
            var nameField = costBlockMeta.Fields[editItemInfo.NameField];

            var selectColumns = new List<BaseColumnInfo>
            {
                SqlFunctions.Max(editItemInfo.ValueField, costBlockMeta.Name),
                SqlFunctions.Count(editItemInfo.ValueField, true, costBlockMeta.Name)
            };

            //ColumnInfo nameColumn;
            ColumnInfo[] groupByColumns;
            Func<SqlHelper, Task<IEnumerable<EditItem>>> readEditItemsFn;

            var nameRefField = nameField as ReferenceFieldMeta;
            if (nameRefField == null)
            {
                var nameColumn = new ColumnInfo(editItemInfo.NameField, costBlockMeta.Name);
                groupByColumns = new[] { nameColumn };
                readEditItemsFn = this.ReadSimpleEditItems;

                selectColumns.Add(nameColumn);
            }
            else
            {
                var nameColumn = new ColumnInfo(nameRefField.FaceValueField, nameRefField.ReferenceMeta.Name);
                groupByColumns = new[] 
                {
                    new ColumnInfo(nameRefField.ValueField, nameRefField.ReferenceMeta.Name),
                    nameColumn
                };
                readEditItemsFn = this.ReadReferenceEditItems;

                selectColumns.Add(nameColumn);
                selectColumns.Add(new ColumnInfo(nameRefField.ValueField, nameRefField.ReferenceMeta.Name));
            }

            var fromQuery = Sql.Select(selectColumns.ToArray()).From(costBlockMeta);

            var query = nameRefField == null 
                    ? (IWhereSqlHelper<IGroupBySqlHelper<SqlHelper>>)fromQuery 
                    : fromQuery.Join(costBlockMeta, nameRefField.Name);

            var resultQuery = query.Where(filter, costBlockMeta.Name).GroupBy(groupByColumns);

            return await readEditItemsFn(resultQuery);
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo)
        {
            var costBlockMeta = this.domainEnitiesMeta.GetEntityMeta(editItemInfo.EntityName, editItemInfo.Schema);
            var nameField = costBlockMeta.Fields[editItemInfo.NameField];

            Func<EditItem, int, SqlHelper> queryFn;

            var nameRefField = nameField as ReferenceFieldMeta;
            if (nameRefField == null)
            {
                queryFn = (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo, index, editItem.Name);
            }
            else
            {
                queryFn = (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo, index, editItem.Id);
            }

            var query = Sql.Queries(editItems.Select(queryFn));


            return await this.repositorySet.ExecuteSql(query);
        }

        private SqlHelper BuildUpdateValueQuery(EditItem editItem, EditItemInfo editItemInfo, int index, object value)
        {
            var updateColumn = new ValueUpdateColumnInfo(
                editItemInfo.ValueField,
                editItem.Value,
                $"{editItemInfo.ValueField}_{index}");

            return Sql.Update(editItemInfo.Schema, editItemInfo.EntityName, updateColumn)
                      .Where(SqlOperators.Equals(editItemInfo.NameField, $"param_{index}", value));
        }

        private async Task<IEnumerable<EditItem>> ReadSimpleEditItems(SqlHelper query)
        {
            var editItems = await this.repositorySet.ReadBySql(query, this.BuildSimpleEditItem);

            return editItems.Select((editItem, index) =>
            {
                editItem.Id = index;

                return editItem;
            });
        }

        private async Task<IEnumerable<EditItem>> ReadReferenceEditItems(SqlHelper query)
        {
            return await this.repositorySet.ReadBySql(query, this.BuildReferenceEditItem);
        }

        private EditItem BuildSimpleEditItem(IDataReader reader)
        {
            return new EditItem
            {
                Value = reader.GetDouble(0),
                ValueCount = reader.GetInt32(1),
                Name = reader[2].ToString(),
            };
        }

        private EditItem BuildReferenceEditItem(IDataReader reader)
        {
            var editItem = this.BuildSimpleEditItem(reader);
            editItem.Id = reader.GetInt64(3);

            return editItem;
        }
    }
}
