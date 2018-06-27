using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class SqlOperators
    {
        public static ConditionHelper Equals(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<EqualsSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Equals(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<EqualsSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper Equals(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<EqualsSqlBuilder>(new ColumnSqlBuilder(leftColumn), new ColumnSqlBuilder(rightColumn));
        }

        public static ConditionHelper NotEquals(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper NotEquals(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static T BinaryOperator<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binaryOperator = new T
            {
                LeftOperator = leftOperand,
                RightOperator = rightOperand
            };

            return binaryOperator;
        }

        private static ConditionHelper CreateConditionHelper<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return new ConditionHelper(BinaryOperator<T>(leftOperand, rightOperand));
        }

        private static ConditionHelper CreateConditionHelper<T>(string columnName, string paramName, object value, string tableName)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binOperator = BinaryOperator<T>(
                new ColumnSqlBuilder { Name = columnName, Table = tableName },
                new ParameterSqlBuilder
                {
                    ParamInfo = new CommandParameterInfo { Name = paramName, Value = value }
                });

            var result = new ConditionHelper(binOperator);

            return result;
        }
    }
}
