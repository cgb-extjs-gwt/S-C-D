using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class SqlFunctions
    {
        public static QueryColumnInfo Max(ISqlBuilder query, string alias = null)
        {
            return CreateQueryColumnInfo<MaxSqlBuilder>(query, alias);
        }

        public static QueryColumnInfo Max(string columnName, string tableName = null, string alias = null)
        {
            return CreateQueryColumnInfo<MaxSqlBuilder>(columnName, tableName, alias);
        }

        public static QueryColumnInfo Max(FieldMeta field, string tableName = null, string alias = null)
        {
            return field is SimpleFieldMeta simpleField && simpleField.Type == TypeCode.Boolean
                ? Max(
                    Convert(new ColumnSqlBuilder(simpleField.Name, tableName), TypeCode.Int32),
                    alias)
                : Max(field.Name, tableName, alias);
        }

        public static QueryColumnInfo Min(ISqlBuilder query, string alias = null)
        {
            return CreateQueryColumnInfo<MinSqlBuilder>(query, alias);
        }

        public static QueryColumnInfo Min(string columnName, string tableName = null, string alias = null)
        {
            return CreateQueryColumnInfo<MinSqlBuilder>(columnName, tableName, alias);
        }

        public static QueryColumnInfo Min(FieldMeta field, string tableName = null, string alias = null)
        {
            return field is SimpleFieldMeta simpleField && simpleField.Type == TypeCode.Boolean
                ? Min(
                    Convert(new ColumnSqlBuilder(simpleField.Name, tableName), TypeCode.Int32),
                    alias)
                : Min(field.Name, tableName, alias);
        }

        public static QueryColumnInfo Count(string columnName = null, bool isDisctinct = false, string tableName = null, string alias = null)
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

        public static QueryColumnInfo Average(ISqlBuilder query, string alias = null)
        {
            return CreateQueryColumnInfo<AverageSqlBuilder>(query, alias);
        }

        public static QueryColumnInfo Average(string columnName, string tableName = null, string alias = null)
        {
            return CreateQueryColumnInfo<AverageSqlBuilder>(columnName, tableName, alias);
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

        public static QueryColumnInfo RowNumber(SortInfo sortInfo, string alias = null)
        {
            return new QueryColumnInfo
            {
                Alias = alias,
                Query = new RowNumberColumnSqlBuilder
                {
                    OrderByInfos = new[]
                    {
                        new OrderByInfo
                        {
                            Direction = sortInfo.Direction,
                            SqlBuilder = new ColumnSqlBuilder(sortInfo.Property)
                        }
                    }
                }
            };
        }

        public static QueryColumnInfo Value(object value, string alias = null, TypeCode? type = null)
        {
            var valueSqlBuilder = new ValueSqlBuilder(value);

            return new QueryColumnInfo(
                type.HasValue ? Convert(valueSqlBuilder, type.Value) : (ISqlBuilder)valueSqlBuilder,
                alias);
        }

        public static QueryColumnInfo Case(string alias, IEnumerable<CaseItem> caseItems, ISqlBuilder elseQuery = null, ISqlBuilder inputQuery = null)
        {
            return new QueryColumnInfo(
                new CaseSqlBuilder
                {
                    Input = inputQuery,
                    Cases = caseItems,
                    Else = elseQuery
                },
                alias);
        }

        public static QueryColumnInfo IfElse(string alias, ConditionHelper whenQuery, ISqlBuilder thenQuery, ISqlBuilder elseQuery = null)
        {
            return Case(
                alias,
                new[]
                {
                    new CaseItem
                    {
                        When = whenQuery.ToSqlBuilder(),
                        Then = thenQuery
                    }
                },
                elseQuery);
        }

        private static QueryColumnInfo CreateQueryColumnInfo<T>(ISqlBuilder query, string alias = null) 
            where T : BaseQuerySqlBuilder, new()
        {
            return new QueryColumnInfo
            {
                Alias = alias,
                Query = new T
                {
                    Query = query
                }
            };
        }

        private static QueryColumnInfo CreateQueryColumnInfo<T>(string columnName, string tableName = null, string alias = null)
            where T : BaseQuerySqlBuilder, new()
        {
            var column = new ColumnSqlBuilder
            {
                Table = tableName,
                Name = columnName
            };

            return CreateQueryColumnInfo<T>(column, alias);
        }
    }
}
