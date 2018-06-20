using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Constants;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostEditorRepository : ICostEditorRepository
    {
        private readonly IRepositorySet repositorySet;

        public CostEditorRepository(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var query =
                Sql.Select(DataBaseConstants.IdFieldName, editItemInfo.NameColumn, editItemInfo.ValueColumn)
                         .From(editItemInfo.TableName, editItemInfo.SchemaName)
                         .Where(filter);

            return await this.repositorySet.ReadBySql(
                query, 
                reader => new EditItem
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Value = reader.GetDouble(2),
                    ValueCount = 1
                });
        }

        public async Task<IEnumerable<EditItem>> GetEditItemsByLevel(string levelColumnName, EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var nameColumn = new ColumnInfo(editItemInfo.NameColumn);
            var maxValueColumn = SqlFunctions.Max(editItemInfo.ValueColumn);
            var countDiffValues = SqlFunctions.Count(editItemInfo.ValueColumn);
            var levelColumn = new ColumnInfo(levelColumnName);

            var query =
                Sql.Select(nameColumn, maxValueColumn, countDiffValues)
                   .From(editItemInfo.TableName, editItemInfo.SchemaName)
                   .Where(filter)
                   .GroupBy(levelColumn);

            return await this.repositorySet.ReadBySql(
                query,
                reader => new EditItem
                {
                    Name = reader.GetString(0),
                    Value = reader.GetDouble(1),
                    ValueCount = reader.GetInt32(2)
                });
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo)
        {
            var query = Sql.Queries(
                editItems.Select(
                    (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo)
                                             .Where(SqlOperators.Equals(DataBaseConstants.IdFieldName, $"id_{index}", editItem.Id))));

            return await this.repositorySet.ExecuteSql(query);
        }

        public async Task<int> UpdateValuesByLevel(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo, string levelColumnName)
        {
            var query = Sql.Queries(
                editItems.Select(
                    (editItem, index) => this.BuildUpdateValueQuery(editItem, editItemInfo)
                                             .Where(SqlOperators.Equals(levelColumnName, $"param_{index}", editItem.Name))));

            return await this.repositorySet.ExecuteSql(query);
        }

        private UpdateSqlHelper BuildUpdateValueQuery(EditItem editItem, EditItemInfo editItemInfo)
        {
            return Sql.Update(
                editItemInfo.SchemaName,
                editItemInfo.TableName,
                new ValueUpdateColumnInfo(editItemInfo.ValueColumn, editItem.Value));
        }
    }
}
