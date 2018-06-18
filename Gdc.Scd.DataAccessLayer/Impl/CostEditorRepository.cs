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

            return await this.repositorySet.ReadFromDb(
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

            return await this.repositorySet.ReadFromDb(
                query,
                reader => new EditItem
                {
                    Name = reader.GetString(0),
                    Value = reader.GetInt32(1),
                    ValueCount = reader.GetInt32(2)
                });
        }
    }
}
