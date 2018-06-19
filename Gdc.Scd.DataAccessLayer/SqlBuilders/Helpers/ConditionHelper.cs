using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class ConditionHelper : SqlHelper
    {
        public ConditionHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public static ConditionHelper And(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<AndSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper And(SqlHelper leftOperand, SqlHelper rightOperand)
        {
            return CreateConditionHelper<AndSqlBuilder>(leftOperand.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public static ConditionHelper And(IEnumerable<ISqlBuilder> operands)
        {
            return CreateConditionHelper<AndSqlBuilder>(operands);
        }

        public static ConditionHelper AndBrackets(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper AndBrackets(SqlHelper leftOperand, SqlHelper rightOperand)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(leftOperand.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public static ConditionHelper Or(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper Or(SqlHelper leftOperand, SqlHelper rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(leftOperand.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public static ConditionHelper Or(IEnumerable<ISqlBuilder> operands)
        {
            return CreateConditionHelper<OrSqlBuilder>(operands);
        }

        public static ConditionHelper OrBrackets(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper OrBrackets(SqlHelper leftOperand, SqlHelper rightOperand)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(leftOperand.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public ConditionHelper And(ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<AndSqlBuilder>(this.ToSqlBuilder(), rightOperand);
        }

        public ConditionHelper And(SqlHelper rightOperand)
        {
            return CreateConditionHelper<AndSqlBuilder>(this.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public ConditionHelper AndBrackets(ISqlBuilder rightOperand)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(this.ToSqlBuilder(), rightOperand);
        }

        public ConditionHelper AndBrackets(SqlHelper rightOperand)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(this.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public ConditionHelper Or(ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperand);
        }

        public ConditionHelper Or(SqlHelper rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public ConditionHelper OrBrackets(ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperand);
        }

        public ConditionHelper OrBrackets(SqlHelper rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        private static ConditionHelper CreateConditionHelper<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return new ConditionHelper(SqlOperators.BinaryOperator<T>(leftOperand, rightOperand));
        }

        private static ConditionHelper CreateConditionHelper<T>(IEnumerable<ISqlBuilder> operands)
            where T : BinaryOperatorSqlBuilder, new()
        {
            ConditionHelper result;

            var operandArray = operands.ToArray();
            if (operandArray.Length == 0)
            {
                result = new ConditionHelper(new RawSqlBuilder { RawSql = string.Empty });
            }
            else
            {
                if (operandArray.Length == 1)
                {
                    result = new ConditionHelper(operandArray[0]);
                }
                else
                {
                    var operand = operandArray[0];

                    for (var i = 2; i < operandArray.Length; i++)
                    {
                        operand = SqlOperators.BinaryOperator<T>(operand, operandArray[i]);
                    }

                    result = new ConditionHelper(operand);
                }
            }

            return result;
        }

        private static ConditionHelper CreateConditionHelperBrackets<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand) 
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binOperator = SqlOperators.BinaryOperator<T>(leftOperand, rightOperand);

            return new ConditionHelper(new BracketsSqlBuilder
            {
                SqlBuilder = binOperator
            });
        }
}
}
