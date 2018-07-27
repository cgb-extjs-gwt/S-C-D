using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
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
            var costBlockMeta = (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(editItemInfo.EntityName, editItemInfo.Schema);
            var nameField = costBlockMeta.InputLevelFields[editItemInfo.NameField];

            var nameColumn = new ColumnInfo(nameField.ReferenceFaceField, nameField.ReferenceMeta.Name);
            var nameIdColumn = new ColumnInfo(nameField.ReferenceValueField, nameField.ReferenceMeta.Name);
            var maxValueColumn = SqlFunctions.Max(editItemInfo.ValueField, costBlockMeta.Name);
            var countColumn = SqlFunctions.Count(editItemInfo.ValueField, true, costBlockMeta.Name);

            var query = 
                Sql.Select(nameIdColumn, nameColumn, maxValueColumn, countColumn)
                   .From(costBlockMeta)
                   .Join(costBlockMeta, nameField.Name)
                   .Where(filter, costBlockMeta.Name)
                   .GroupBy(nameColumn, nameIdColumn);


            return await this.repositorySet.ReadBySql(query, reader => 
            {
                var valueCount = reader.GetInt32(3);

                return new EditItem
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Value = valueCount == 1 ? reader.GetValue(2) : null,
                    ValueCount = valueCount,
                };
            });
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var costBlockMeta = (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(editItemInfo.EntityName, editItemInfo.Schema);
            var nameField = costBlockMeta.InputLevelFields[editItemInfo.NameField];

            //Func<EditItem, int, SqlHelper> queryFn;

            //var nameRefField = nameField as ReferenceFieldMeta;
            //if (nameRefField == null)
            //{
            //    queryFn = (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo, index, editItem.Name, filter);
            //}
            //else
            //{
            //    queryFn = (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo, index, editItem.Id, filter);
            //}

            var query = 
                Sql.Queries(
                    editItems.Select(
                        (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo, index, filter)));

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        private SqlHelper BuildUpdateValueQuery(
            EditItem editItem,
            EditItemInfo editItemInfo, 
            int index, 
            IDictionary<string, IEnumerable<object>> filter = null)
        {
            var updateColumn = new ValueUpdateColumnInfo(
                editItemInfo.ValueField,
                editItem.Value,
                $"{editItemInfo.ValueField}_{index}");

            filter = new Dictionary<string, IEnumerable<object>>(filter ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<object>>>())
            {
                [editItemInfo.NameField] = new object [] 
                {
                    new CommandParameterInfo
                    {
                        Name = $"{editItemInfo.NameField}_{index}",
                        Value = editItem.Id
                    }
                }
            };

            return Sql.Update(editItemInfo.Schema, editItemInfo.EntityName, updateColumn)
                      .Where(filter);
        }

        //private async Task<IEnumerable<EditItem>> ReadSimpleEditItems(SqlHelper query)
        //{
        //    var editItems = await this.repositorySet.ReadBySql(query, this.BuildSimpleEditItem);

        //    return editItems.Select((editItem, index) =>
        //    {
        //        editItem.Id = index;

        //        return editItem;
        //    });
        //}

        //private async Task<IEnumerable<EditItem>> ReadReferenceEditItems(SqlHelper query)
        //{
        //    return await this.repositorySet.ReadBySql(query, this.BuildReferenceEditItem);
        //}

        //private EditItem BuildSimpleEditItem(IDataReader reader)
        //{
        //    var valueCount = reader.GetInt32(1);

        //    return new EditItem
        //    {
        //        Value = valueCount == 1 ? reader.GetValue(0) : null,
        //        ValueCount = valueCount,
        //        Name = reader[2].ToString(),
        //    };
        //}

        //private EditItem BuildReferenceEditItem(IDataReader reader)
        //{
        //    var valueCount = reader.GetInt32(3);

        //    return new EditItem
        //    {
        //        Id = reader.GetInt64(0),
        //        Name = reader.GetString(1),
        //        Value = valueCount == 1 ? reader.GetValue(3) : null,
        //        ValueCount = valueCount,
        //    };
        //}
    }
}
