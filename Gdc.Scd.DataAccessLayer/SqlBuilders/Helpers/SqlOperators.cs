using System;
using System.Linq;
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

        public static ConditionHelper Equals(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<EqualsSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Equals(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<EqualsSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper NotEquals(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper NotEquals(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper NotEquals(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Greater(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<GreaterSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Greater(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<GreaterSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Greater(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<GreaterSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper GreaterOrEqual(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<GreaterOrEqualSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper GreaterOrEqual(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<GreaterOrEqualSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper GreaterOrEqual(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<GreaterOrEqualSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Less(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<LessSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Less(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<LessSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Less(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<LessSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper LessOrEqual(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<LessOrEqualSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper LessOrEqual(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<LessOrEqualSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper LessOrEqual(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<LessOrEqualSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Add(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<AdditionSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Add(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<AdditionSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Add(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<AdditionSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Add(params ISqlBuilder[] operands)
        {
            return CreateConditionHelper<AdditionSqlBuilder>(operands);
        }

        public static ConditionHelper Subtract(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Subtract(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Subtract(params ISqlBuilder[] operands)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(operands);
        }

        public static ConditionHelper Subtract(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<SubtractionSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Multiply(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Multiply(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Multiply(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper Multiply(params ISqlBuilder[] operands)
        {
            return CreateConditionHelper<MultiplicationSqlBuilder>(operands);
        }

        public static ConditionHelper Devide(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<DivisionSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Devide(string columnName, object value = null, string tableName = null)
        {
            return CreateConditionHelper<DivisionSqlBuilder>(columnName, value, tableName);
        }

        public static ConditionHelper Devide(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
        {
            return CreateConditionHelper<DivisionSqlBuilder>(leftColumn, rightColumn);
        }

        public static ConditionHelper IsNull(ISqlBuilder operand)
        {
            var isNull = new IsNullSqlBuilder
            {
                Query = operand
            };

            return new ConditionHelper(isNull);
        }

        public static ConditionHelper IsNull(ColumnInfo column)
        {
            return IsNull(new ColumnSqlBuilder(column));
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

        public static ConditionHelper IsNotNull(ColumnInfo column)
        {
            return IsNotNull(new ColumnSqlBuilder(column));
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

        public static ConditionHelper Between(ISqlBuilder column, ISqlBuilder begin, ISqlBuilder end, bool isNot = false)
        {
            return new ConditionHelper(new BetweenSqlBuilder
            {
                Column = column,
                Begin = begin,
                End = end,
                IsNot = isNot
            });
        }

        public static ConditionHelper Between(string column, int begin, int end, bool isNot = false, string table = null)
        {
            var columnSqlBuilder = new ColumnSqlBuilder(column, table);
            var beginSqlBuilder = new RawSqlBuilder(begin.ToString());
            var endSqlBuilder = new RawSqlBuilder(end.ToString());

            return Between(columnSqlBuilder, beginSqlBuilder, endSqlBuilder, isNot);
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

        private static ConditionHelper CreateConditionHelper<T>(BaseColumnInfo leftColumn, BaseColumnInfo rightColumn)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var leftBuilder = GetColumnSqlBuilder(leftColumn);
            var rigthBuilder = GetColumnSqlBuilder(rightColumn);

            return CreateConditionHelper<T>(leftBuilder, rigthBuilder);

            ISqlBuilder GetColumnSqlBuilder(BaseColumnInfo baseColumnInfo)
            {
                ISqlBuilder result;

                switch (baseColumnInfo)
                {
                    case ColumnInfo column:
                        result = new ColumnSqlBuilder(column);
                        break;

                    case QueryColumnInfo queryColumn:
                        result = new BracketsSqlBuilder
                        {
                            Query = queryColumn.Query
                        };
                        break;

                    default:
                        throw new NotSupportedException();
                }

                return result;
            }
        }

        private static ConditionHelper CreateConditionHelper<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return new ConditionHelper(BinaryOperator<T>(leftOperand, rightOperand));
        }

        private static ConditionHelper CreateConditionHelper<T>(params ISqlBuilder[] operands)
            where T : BinaryOperatorSqlBuilder, new()
        {
            ISqlBuilder result;

            switch (operands.Length)
            {
                case 0:
                    throw new NotSupportedException("Zero operands not supported");

                case 1:
                    result = operands[0];
                    break;

                default:
                    result =
                        operands.Skip(2)
                                .Aggregate(
                                    BinaryOperator<T>(operands[0], operands[1]),
                                    (acc, operand) => new T { LeftOperator = acc, RightOperator = operand });
                    break;
            }

            return new ConditionHelper(result);
        }

        private static ConditionHelper CreateConditionHelper<T>(string columnName, object value, string tableName)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binOperator = BinaryOperator<T>(
                new ColumnSqlBuilder { Name = columnName, Table = tableName },
                new ParameterSqlBuilder
                {
                    ParamInfo = new CommandParameterInfo(value)
                });

            var result = new ConditionHelper(binOperator);

            return result;
        }
    }
}
