using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public static class OperatorsHelper
    {
        public static ConditionHelper Equals(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
        {
            return CreateConditionHelper<EqualsSqlBuilder>(leftOperator, rightOperator);
        }

        public static ConditionHelper Equals(string columnName, string paramName, string tableName = null)
        {
            return Equals(
                new ColumnSqlBuilder { Name = columnName, Table = tableName },
                new ParameterSqlBuilder { Name = paramName });
        }

        public static ConditionHelper NotEquals(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
        {
            return CreateConditionHelper<NotEqualsSqlBuilder>(leftOperator, rightOperator);
        }

        public static ConditionHelper NotEquals(string columnName, string paramName, string tableName = null)
        {
            return NotEquals(
                new ColumnSqlBuilder { Name = columnName, Table = tableName },
                new ParameterSqlBuilder { Name = paramName });
        }

        public static T BinaryOperator<T>(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binaryOperator = new T();
            binaryOperator.LeftOperator = leftOperator;
            binaryOperator.RightOperator = rightOperator;

            return binaryOperator;
        }

        private static ConditionHelper CreateConditionHelper<T>(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return new ConditionHelper(BinaryOperator<T>(leftOperator, rightOperator));
        }
    }
}
