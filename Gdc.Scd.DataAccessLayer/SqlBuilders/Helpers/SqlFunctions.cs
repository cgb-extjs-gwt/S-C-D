using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class SqlFunctions
    {
        public static QueryColumnInfo Max(string columnName, string tableName = null, string alias = null)
        {
            return new QueryColumnInfo
            {
                Query = new MaxSqlBuilder
                {
                    TableName = tableName,
                    ColumnName = columnName
                },
                Alias = alias
            };
        }

        public static QueryColumnInfo Count(string columnName, bool isDisctinct = false, string tableName = null, string alias = null)
        {
            return new QueryColumnInfo
            {
                Query = new CountSqlBuilder
                {
                    TableName = tableName,
                    ColumnName = columnName,
                    IsDisctinct = isDisctinct
                },
                Alias = alias
            };
        }
    }
}
