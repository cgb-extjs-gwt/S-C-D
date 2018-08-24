using System;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class SqlFunctions
    {
        public static QueryColumnInfo Max(ISqlBuilder query, string alias = null)
        {
            return new QueryColumnInfo
            {
                Alias = alias,
                Query = new MaxSqlBuilder
                {
                    Query = query
                }
            };
        }

        public static QueryColumnInfo Max(string columnName, string tableName = null, string alias = null)
        {
            var column = new ColumnSqlBuilder
            {
                Table = tableName,
                Name = columnName
            };

            return Max(column, alias);
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

        public static ConvertSqlBuilder Convert(ISqlBuilder query, TypeCode type)
        {
            return new ConvertSqlBuilder
            {
                Query = query,
                Type = type
            };
        }

        public static QueryColumnInfo Convert(string columnName, TypeCode type, string tableName = null, string alias = null)
        {
            var column = new ColumnSqlBuilder
            {
                Table = tableName,
                Name = columnName
            };

            return new QueryColumnInfo
            {
                Alias = alias,
                Query = Convert(column, type)
            };
        }
    }
}
