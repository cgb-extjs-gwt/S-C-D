using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostEditorRepository : ICostEditorRepository
    {
        private readonly IRepositorySet repositorySet;

        public CostEditorRepository(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public async Task<IEnumerable<EditItem>> GetEditItemsByLevel(string levelName, EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var nameColumn = new ColumnInfo(editItemInfo.NameColumn);
            var maxValueColumn = SqlFunctions.Max(editItemInfo.ValueColumn);
            var countDiffValues = SqlFunctions.Count(editItemInfo.ValueColumn, true);
            var levelColumn = new ColumnInfo(levelName);

            var query =
                Sql.Select(nameColumn, maxValueColumn, countDiffValues)
                   .From(editItemInfo.EntityName, editItemInfo.Schema)
                   .Where(filter)
                   .GroupBy(levelColumn);

            var editItems = await this.repositorySet.ReadBySql(
                query,
                reader => new EditItem
                {
                    Name = reader.GetString(0),
                    Value = reader.GetDouble(1),
                    ValueCount = reader.GetInt32(2)
                });

            return editItems.Select((editItem, index) =>
            {
                editItem.Id = index;

                return editItem;
            });
        }

        public async Task<int> UpdateValuesByLevel(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo, string levelColumnName)
        {
            var query = Sql.Queries(
                editItems.Select(
                    (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo, index)
                                             .Where(SqlOperators.Equals(levelColumnName, $"param_{index}", editItem.Name))));

            return await this.repositorySet.ExecuteSql(query);
        }

        private UpdateSqlHelper BuildUpdateValueQuery(EditItem editItem, EditItemInfo editItemInfo, int paramIndex)
        {
            return Sql.Update(
                editItemInfo.Schema,
                editItemInfo.EntityName,
                new ValueUpdateColumnInfo(
                    editItemInfo.ValueColumn, 
                    editItem.Value, 
                    $"{editItemInfo.ValueColumn}_{paramIndex}"));
        }
    }
}
