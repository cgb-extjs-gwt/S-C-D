using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class ConditionHelper : BaseSqlHelper
    {
        public ConditionHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public static ConditionHelper And(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
        {
            return CreateConditionHelper<AndSqlBuilder>(leftOperator, rightOperator);
        }

        public static ConditionHelper AndBrackets(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(leftOperator, rightOperator);
        }

        public static ConditionHelper Or(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
        {
            return CreateConditionHelper<OrSqlBuilder>(leftOperator, rightOperator);
        }

        public static ConditionHelper OrBrackets(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(leftOperator, rightOperator);
        }

        public ConditionHelper And(ISqlBuilder rightOperator)
        {
            return CreateConditionHelper<AndSqlBuilder>(this.ToSqlBuilder(), rightOperator);
        }

        public ConditionHelper AndBrackets(ISqlBuilder rightOperator)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(this.ToSqlBuilder(), rightOperator);
        }

        public ConditionHelper Or(ISqlBuilder rightOperator)
        {
            return new ConditionHelper(OperatorsHelper.BinaryOperator<OrSqlBuilder>(this.ToSqlBuilder(), rightOperator));
        }

        public ConditionHelper OrBrackets(ISqlBuilder rightOperator)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperator);
        }

        private static ConditionHelper CreateConditionHelper<T>(ISqlBuilder leftOperator, ISqlBuilder rightOperator)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return new ConditionHelper(OperatorsHelper.BinaryOperator<T>(leftOperator, rightOperator));
        }

        private static ConditionHelper CreateConditionHelperBrackets<T>(ISqlBuilder leftOperator, ISqlBuilder rightOperator) 
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binOperator = OperatorsHelper.BinaryOperator<T>(leftOperator, rightOperator);

            return new ConditionHelper(new BracketsSqlBuilder
            {
                SqlBuilder = binOperator
            });
        }
    }
}
