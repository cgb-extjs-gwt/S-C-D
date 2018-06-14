using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class UpdateSqlBuilder : ISqlBuilder
    {
        public string DataBaseName { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public IEnumerable<UpdateColumnInfo> Columns { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var tableNameBuilder = new TableSqlBuilder
            {
                DataBase = this.DataBaseName,
                Schema = this.SchemaName,
                Name = this.TableName
            };
            var table = tableNameBuilder.Build(context);

            var columns = string.Join(
                ", ", 
                this.Columns.Select(columnInfo => $"{columnInfo.Name} = {columnInfo.Query.Build(context)}"));

            return $"UPDATE {table} SET {columns}";
        }
    }
}
