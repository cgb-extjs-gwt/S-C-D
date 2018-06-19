using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class Sql
    {
        public static SqlHelper Queries(IEnumerable<ISqlBuilder> queries)
        {
            return new SqlHelper(new SeveralQuerySqlBuilder { Queries = queries });
        }

        public static SqlHelper Queries(IEnumerable<SqlHelper> queries)
        {
            return new SqlHelper(new SeveralQuerySqlBuilder
            {
                Queries = queries.Select(query => query.ToSqlBuilder())
            });
        }

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

        public static UpdateSqlHelper Update(string dataBase, string schema, string table, params BaseUpdateColumnInfo[] columns)
        {
            var queryColumns = new List<QueryUpdateColumnInfo>();

            foreach (var column in columns)
            {
                var queryColumn = column as QueryUpdateColumnInfo;
                if (queryColumn == null)
                {
                    var valueColumn = (ValueUpdateColumnInfo)column;

                    queryColumn = new QueryUpdateColumnInfo(
                        valueColumn.Name,
                        new ParameterSqlBuilder
                        {
                            ParamInfo = new CommandParameterInfo
                            {
                                Name = valueColumn.Name,
                                Value = valueColumn.Value
                            }
                        });
                }

                queryColumns.Add(queryColumn);
            }

            var updateBuilder = new UpdateSqlBuilder()
            {
                DataBaseName = dataBase,
                SchemaName = schema,
                TableName = table,
                Columns = queryColumns
            };

            return new UpdateSqlHelper(updateBuilder);
        }

        public static UpdateSqlHelper Update(string schema, string table, params BaseUpdateColumnInfo[] columns)
        {
            return Update(null, schema, table, columns);
        }

        public static UpdateSqlHelper Update(string table, params BaseUpdateColumnInfo[] columns)
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
