using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

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
                   .Where(this.BuildCostEditorWhereCondition(costBlockMeta, filter, costBlockMeta.Name))
                   .GroupBy(nameColumn, nameIdColumn);


            return await this.repositorySet.ReadBySql(query, reader => 
            {
                var valueCount = reader.GetInt32(3);

                return new EditItem
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Value = valueCount == 1 ? reader.GetValue(2) : 0,
                    ValueCount = valueCount,
                };
            });
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var costBlockMeta = (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(editItemInfo.EntityName, editItemInfo.Schema);
            var nameField = costBlockMeta.InputLevelFields[editItemInfo.NameField];

            var query = 
                Sql.Queries(
                    editItems.Select(
                        (editItem, index) => this.BuildUpdateValueQuery(costBlockMeta, editItem, editItemInfo, index, filter)));

            return await this.repositorySet.ExecuteSqlAsync(query);
        }

        private SqlHelper BuildUpdateValueQuery(
            CostBlockEntityMeta meta,
            EditItem editItem,
            EditItemInfo editItemInfo, 
            int index, 
            IDictionary<string, IEnumerable<object>> filter = null)
        {
            var updateColumn = new ValueUpdateColumnInfo(
                editItemInfo.ValueField,
                editItem.Value,
                $"{editItemInfo.ValueField}_{index}");

            //filter = new Dictionary<string, IEnumerable<object>>(filter ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<object>>>())
            filter = new Dictionary<string, IEnumerable<object>>(filter ?? new Dictionary<string, IEnumerable<object>>())       
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
                      .Where(this.BuildCostEditorWhereCondition(meta, filter));
        }

        private ConditionHelper BuildCostEditorWhereCondition(CostBlockEntityMeta meta, IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            ConditionHelper result;

            var notDeletedCondition = SqlOperators.IsNull(meta.DeletedDateField.Name, tableName);

            if (filter != null && filter.Count > 0)
            {
                result = ConditionHelper.AndStatic(filter, tableName).And(notDeletedCondition);
            }
            else
            {
                result = notDeletedCondition;
            }

            return result;
        }
    }
}
