using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class InsertSqlHelper : SqlHelper
    {
        public InsertSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SqlHelper Values(ISqlBuilder[,] queries)
        {
            return new SqlHelper(new InsertValuesSqlBuilder
            {
                Queries = queries,
                SqlBuilder = this.ToSqlBuilder()
            });
        }

        public SqlHelper Values(params ISqlBuilder[] row)
        {
            var rows = this.ConvertToTable(row);

            return this.Values(rows);
        }

        public SqlHelper Values(object[,] values, string paramPrefix = null)
        {
            if (string.IsNullOrEmpty(paramPrefix))
            {
                paramPrefix = "param";
            }

            var rowLength = values.GetLength(0);
            var columnLength = values.GetLength(1);
            var parameters = new ParameterSqlBuilder[rowLength, columnLength];

            for (var rowIndex = 0; rowIndex < rowLength; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < columnLength; columnIndex++)
                {
                    parameters[rowIndex, columnIndex] = new ParameterSqlBuilder
                    {
                        ParamInfo = new CommandParameterInfo
                        {
                            Name = $"{paramPrefix}_{rowIndex}_{columnIndex}",
                            Value = values[rowIndex, columnIndex]
                        }
                    };
                }
            }

            return this.Values(parameters);
        }

        public SqlHelper Values(params object[] values)
        {
            var rows = this.ConvertToTable(values);

            return this.Values(rows);
        }

        public SqlHelper Values(string paramPrefix, params object[] values)
        {
            var rows = this.ConvertToTable(values);

            return this.Values(rows, paramPrefix);
        }

        public SqlHelper Values(IEnumerable<object[]> rows, string paramPrefix = null)
        {
            var rowArray = rows.ToArray();
            var valueArray = new object[rowArray.Length, rows.Max(arr => arr.Length)];

            for (var rowIndex = 0; rowIndex < rowArray.Length; rowIndex++)
            {
                var row = rowArray[rowIndex];

                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    valueArray[rowIndex, columnIndex] = row[columnIndex];
                }
            }

            return this.Values(valueArray, paramPrefix);
        }

        public SqlHelper Query(ISqlBuilder query)
        {
            return new SqlHelper(new InsertQuerySqlBuilder
            {
                Query = query,
                SqlBuilder = this.ToSqlBuilder()
            });
        }

        public SqlHelper Query(SqlHelper query)
        {
            return this.Query(query.ToSqlBuilder());
        }

        private T[,] ConvertToTable<T>(T[] row)
        {
            var table = new T[1, row.Length];

            for (var index = 0; index < row.Length; index++)
            {
                table[0, index] = row[index];
            }

            return table;
        }
    }
}
