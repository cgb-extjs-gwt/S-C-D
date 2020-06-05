using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
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

        public static ConditionHelper And(IEnumerable<ConditionHelper> operands)
        {
            return CreateConditionHelper<AndSqlBuilder>(operands);
        }

        public static ConditionHelper AndStatic(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return CreateConditionHelper<AndSqlBuilder>(filter, tableName);
        }

        public static ConditionHelper AndStatic(IDictionary<ColumnInfo, IEnumerable<object>> filter)
        {
            return CreateConditionHelper<AndSqlBuilder>(filter);
        }

        public static ConditionHelper AndBrackets(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper AndBrackets(SqlHelper leftOperand, SqlHelper rightOperand)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(leftOperand.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public static ConditionHelper AndBrackets(IEnumerable<ISqlBuilder> operands)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(operands);
        }

        public static ConditionHelper AndBrackets(IEnumerable<ConditionHelper> operands)
        {
            return CreateConditionHelperBrackets<AndSqlBuilder>(operands);
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

        public static ConditionHelper Or(IEnumerable<ConditionHelper> operands)
        {
            return CreateConditionHelper<OrSqlBuilder>(operands);
        }

        public static ConditionHelper OrStatic(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return CreateConditionHelper<OrSqlBuilder>(filter, tableName);
        }

        public static ConditionHelper OrStatic(IDictionary<ColumnInfo, IEnumerable<object>> filter, string tableName = null)
        {
            return CreateConditionHelper<OrSqlBuilder>(filter);
        }

        public static ConditionHelper OrBrackets(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(leftOperand, rightOperand);
        }

        public static ConditionHelper OrBrackets(SqlHelper leftOperand, SqlHelper rightOperand)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(leftOperand.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public static ConditionHelper OrBrackets(IEnumerable<ISqlBuilder> operands)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(operands);
        }

        public static ConditionHelper OrBrackets(IEnumerable<ConditionHelper> operands)
        {
            return CreateConditionHelperBrackets<OrSqlBuilder>(operands);
        }

        public ConditionHelper And(ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<AndSqlBuilder>(this.ToSqlBuilder(), rightOperand);
        }

        public ConditionHelper And(SqlHelper rightOperand)
        {
            return CreateConditionHelper<AndSqlBuilder>(this.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public ConditionHelper And(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return this.And(CreateConditionHelper<AndSqlBuilder>(filter, tableName));
        }

        public ConditionHelper And(IDictionary<ColumnInfo, IEnumerable<object>> filter, string tableName = null)
        {
            return this.And(CreateConditionHelper<AndSqlBuilder>(filter));
        }

        public ConditionHelper AndBrackets(ISqlBuilder rightOperand)
        {
            return this.CreateConditionHelperBrackets<AndSqlBuilder>(rightOperand);
        }

        public ConditionHelper AndBrackets(SqlHelper rightOperand)
        {
            return this.CreateConditionHelperBrackets<AndSqlBuilder>(rightOperand.ToSqlBuilder());
        }

        public ConditionHelper Or(ISqlBuilder rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperand);
        }

        public ConditionHelper Or(SqlHelper rightOperand)
        {
            return CreateConditionHelper<OrSqlBuilder>(this.ToSqlBuilder(), rightOperand.ToSqlBuilder());
        }

        public ConditionHelper Or(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return this.Or(CreateConditionHelper<OrSqlBuilder>(filter, tableName));
        }

        public ConditionHelper Or(IDictionary<ColumnInfo, IEnumerable<object>> filter, string tableName = null)
        {
            return this.Or(CreateConditionHelper<OrSqlBuilder>(filter));
        }

        public ConditionHelper OrBrackets(ISqlBuilder rightOperand)
        {
            return this.CreateConditionHelperBrackets<OrSqlBuilder>(rightOperand);
        }

        public ConditionHelper OrBrackets(SqlHelper rightOperand)
        {
            return this.CreateConditionHelperBrackets<OrSqlBuilder>(rightOperand.ToSqlBuilder());
        }

        private static ConditionHelper CreateConditionHelper<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand)
            where T : BinaryOperatorSqlBuilder, new()
        {
            ConditionHelper result;

            var rawSqlBuilder = leftOperand as RawSqlBuilder;
            if (rawSqlBuilder == null || !string.IsNullOrWhiteSpace(rawSqlBuilder.RawSql))
            {
                result = new ConditionHelper(SqlOperators.BinaryOperator<T>(leftOperand, rightOperand));
            }
            else
            {
                result = new ConditionHelper(rightOperand);
            }

            return result;
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

                    for (var i = 1; i < operandArray.Length; i++)
                    {
                        operand = SqlOperators.BinaryOperator<T>(operand, operandArray[i]);
                    }

                    result = new ConditionHelper(operand);
                }
            }

            return result;
        }

        private static ConditionHelper CreateConditionHelper<T>(IEnumerable<ConditionHelper> operands)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return CreateConditionHelper<T>(operands.Select(operand => operand.ToSqlBuilder()));
        }

        private static ConditionHelper CreateConditionHelperBrackets<T>(IEnumerable<ISqlBuilder> operands)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return new ConditionHelper(new BracketsSqlBuilder
            {
                Query = CreateConditionHelper<T>(operands).ToSqlBuilder()
            });
        }

        private static ConditionHelper CreateConditionHelperBrackets<T>(IEnumerable<ConditionHelper> operands)
            where T : BinaryOperatorSqlBuilder, new()
        {
            return CreateConditionHelperBrackets<T>(operands.Select(operand => operand.ToSqlBuilder()));
        }

        private static ConditionHelper CreateConditionHelperBrackets<T>(ISqlBuilder leftOperand, ISqlBuilder rightOperand) 
            where T : BinaryOperatorSqlBuilder, new()
        {
            var binOperator = SqlOperators.BinaryOperator<T>(leftOperand, rightOperand);

            return new ConditionHelper(new BracketsSqlBuilder
            {
                Query = binOperator
            });
        }

        private static ConditionHelper CreateConditionHelper<T>(IDictionary<ColumnInfo, IEnumerable<object>> filter)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var inBuilders = new List<InSqlBuilder>();

            foreach (var filterItem in filter)
            {
                var parameterBuilders = new List<ParameterSqlBuilder>();

                foreach (var value in filterItem.Value)
                {
                    parameterBuilders.Add(new ParameterSqlBuilder
                    {
                        ParamInfo = new CommandParameterInfo(value)
                    });
                }

                if (parameterBuilders.Count > 0)
                {
                    inBuilders.Add(new InSqlBuilder
                    {
                        Table = filterItem.Key.TableName,
                        Column = filterItem.Key.Name,
                        Values = parameterBuilders
                    });
                }
            }

            return CreateConditionHelper<T>(inBuilders);
        }

        private static ConditionHelper CreateConditionHelper<T>(IDictionary<string, IEnumerable<object>> filter, string tableName)
            where T : BinaryOperatorSqlBuilder, new()
        {
            var columnFilter = filter.ToDictionary(
                keyValue => new ColumnInfo(keyValue.Key, tableName),
                keyValue => keyValue.Value);

            return CreateConditionHelper<T>(columnFilter);
        }

        private ConditionHelper CreateConditionHelperBrackets<T>(ISqlBuilder rightOperand)
            where T : BinaryOperatorSqlBuilder, new()
        {
            rightOperand = new BracketsSqlBuilder
            {
                Query = rightOperand
            };


            return CreateConditionHelper<T>(this.ToSqlBuilder(), rightOperand);
        }
    }
}
