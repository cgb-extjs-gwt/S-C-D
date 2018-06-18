using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class Sql
    {
        public static SelectSqlHelper Select(params BaseColumnInfo[] columns)
        {
            return Select(false, columns);
        }

        public static SelectSqlHelper Select(params string[] columnNames)
        {
            var columns = GetColumnInfos(columnNames).ToArray();

            return Select(false, columns);
        }

        public static SelectSqlHelper SelectDistinct(params string[] columnNames)
        {
            var columns = GetColumnInfos(columnNames).ToArray();

            return Select(true, columns);
        }

        public static SelectSqlHelper SelectDistinct(params BaseColumnInfo[] columns)
        {
            return Select(true, columns);
        }

        public static WhereSqlHelper Update(string dataBase, string schema, string table, params UpdateColumnInfo[] columns)
        {
            return new WhereSqlHelper(new UpdateSqlBuilder
            {
                DataBaseName = dataBase,
                SchemaName = schema,
                TableName = table,
                Columns = columns
            });
        }

        public static WhereSqlHelper Update(string schema, string table, params UpdateColumnInfo[] columns)
        {
            return Update(null, schema, table, columns);
        }

        public static WhereSqlHelper Update(string table, params UpdateColumnInfo[] columns)
        {
            return Update(null, table, columns);
        }

        private static SelectSqlHelper Select(bool isDistinct, params BaseColumnInfo[] columns)
        {
            var selectBuilder = new SelectSqlBuilder
            {
                IsDistinct = isDistinct,
                Columns = columns.Select(GetColumnSqlBuilder)
            };

            return new SelectSqlHelper(selectBuilder);
        }

        private static ISqlBuilder GetColumnSqlBuilder(BaseColumnInfo baseColumnInfo)
        {
            ISqlBuilder builder;

            var queryColumnInfo = baseColumnInfo as QueryColumnInfo;
            if (queryColumnInfo == null)
            {
                var columnInfo = (ColumnInfo)baseColumnInfo;

                builder = new ColumnSqlBuilder
                {
                    Name = columnInfo.Name,
                    Table = columnInfo.TableName
                };

                if (!string.IsNullOrWhiteSpace(baseColumnInfo.Alias))
                {
                    builder = new AliasSqlBuilder
                    {
                        SqlBuilder = builder,
                        Alias = baseColumnInfo.Alias
                    };
                }
            }
            else 
            {
                var bracketsSqlBuilder = new BracketsSqlBuilder
                {
                    SqlBuilder = queryColumnInfo.Query
                };

                if (string.IsNullOrWhiteSpace(baseColumnInfo.Alias))
                {
                    builder = bracketsSqlBuilder;
                }
                else
                {
                    builder = new AliasSqlBuilder
                    {
                        Alias = baseColumnInfo.Alias,
                        SqlBuilder = bracketsSqlBuilder
                    };
                }
            }

            return builder;
        }

        private static IEnumerable<ColumnInfo> GetColumnInfos(IEnumerable<string> columnNames)
        {
            return columnNames.Select(name => new ColumnInfo { Name = name });
        }
    }
}
