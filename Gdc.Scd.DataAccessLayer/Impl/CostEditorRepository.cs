using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Constants;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostEditorRepository
    {
        private readonly EntityFrameworkRepositorySet repositorySet;

        public CostEditorRepository(EntityFrameworkRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public async Task<IEnumerable<string>> GetValues(string columnName, string tableName, string schemaName = null)
        {
            var sql = SqlHelper.SelectDistinct(columnName).From(tableName, schemaName).ToSql();

            return await this.repositorySet.ReadFromDb(sql, reader => reader.GetString(0));
        }

        public async Task<IEnumerable<EditItem>> GetAllEditItems(string nameColumn, string valueColumn, string tableName, string schemaName = null)
        {
            var sql = SqlHelper.Select(DataBaseConstants.IdFieldName, nameColumn, valueColumn).From(tableName).ToSql();

            return await this.repositorySet.ReadFromDb(
                sql, 
                reader => new EditItem
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Value = reader.GetDouble(2)
                });
        }

        public async Task<IEnumerable<EditItem>> GetEditItemsByLevel(string levelColumn, string nameColumn, string valueColumn, string tableName, string schemaName = null)
        {
            var sql = SqlHelper.Select(
                new ColumnInfo(DataBaseConstants.IdFieldName),
                new ColumnInfo(nameColumn),
                new ColumnInfo(valueColumn)
                
                )
        }

        //public async Task<IEnumerable<string>> GetValues(string tableName, string columnName, string schemaName = null)
        //{
        //    var columnInfo = new ColumnInfo { Name = columnName };
        //    var sql = this.BuildSql(tableName, new[] { columnInfo }, schemaName);

        //    return await this.ReadFromDb(sql, reader => reader.GetString(0));
        //}

        //private string BuildSql(string tableName, IEnumerable<ColumnInfo> columnInfos, string schemaName = null)
        //{
        //    var columns = string.Join(", ", columnInfos.Select(this.BuilColumnSql));
        //    var table = this.BuildTableSql(schemaName, tableName);

        //    return $"SELECT {columns} FROM {table}";
        //}

        //private string BuilColumnSql(ColumnInfo columnInfo)
        //{
        //    string result;

        //    if (string.IsNullOrWhiteSpace(columnInfo.Alias))
        //    {
        //        result = $"[{columnInfo.Name}]";
        //    }
        //    else
        //    {
        //        result = $"[{columnInfo.Name}] AS [{columnInfo.Alias}]";
        //    }

        //    return result;
        //}

        //private string BuildTableSql(string schemaName, string tableName)
        //{
        //    string result; 

        //    if (string.IsNullOrWhiteSpace(schemaName))
        //    {
        //        result = $"[{tableName}]";
        //    }
        //    else
        //    {
        //        result = $"[{schemaName}].[{tableName}]";
        //    }

        //    return result;
        //}
    }
}
