using System.Collections.Generic;
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
            return CreateConditionHelper<EqualsSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper NotEquals(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper NotEquals(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper NotEquals(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Greater(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<GreaterSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Greater(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<GreaterSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper Greater(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<GreaterSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper GreaterOrEqual(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<GreaterOrEqualSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper GreaterOrEqual(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<GreaterOrEqualSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper GreaterOrEqual(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<GreaterOrEqualSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Less(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<LessSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Less(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<LessSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper Less(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<LessSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper LessOrEqual(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<LessOrEqualSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper LessOrEqual(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<LessOrEqualSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper LessOrEqual(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<LessOrEqualSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Subtract(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Subtract(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper Subtract(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Multiply(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Multiply(string columnName, string paramName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(columnName, paramName, value, tableName);
        }

        public static ConditionHelper Multiply(ColumnInfo leftColumn, ColumnInfo rightColumn)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper IsNull(ISqlBuilder operand)
        {
            var isNull = new IsNullSqlBuilder
            {
                Query = operand
            };

            return new ConditionHelper(isNull);
        }

        public static ConditionHelper IsNull(string columnName, string tableName = null)
        {
            var column = new ColumnSqlBuilder
            {
                Table = tableName,
                Name = columnName
            };

            return IsNull(column);
        }

        public static ConditionHelper IsNotNull(ISqlBuilder operand)
        {
            var isNotNull = new IsNotNullSqlBuilder
            {
                Query = operand
            };

            return new ConditionHelper(isNotNull);
        }

        public static ConditionHelper IsNotNull(string columnName, string tableName = null)
        {
            var column = new ColumnSqlBuilder
            {
                Table = tableName,
                Name = columnName
            };

            return IsNotNull(column);
        }

        public static ConditionHelper In(string column, ISqlBuilder valuesQuery, string table = null)
        {
            return new ConditionHelper(new InSqlBuilder
            {
                Table = table,
                Column = column,
                Values = new[] { valuesQuery }
            });
        }

        public static ConditionHelper In(string column, SqlHelper valuesQuery, string table = null)
        {
            return In(column, valuesQuery.ToSqlBuilder(), table);
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

        private static ConditionHelper CreateConditionHelper<T>(ColumnInfo leftColumn, ColumnInfo rightColumn)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return CreateConditionHelper<T>(new ColumnSqlBuilder(leftColumn), new ColumnSqlBuilder(rightColumn));
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
