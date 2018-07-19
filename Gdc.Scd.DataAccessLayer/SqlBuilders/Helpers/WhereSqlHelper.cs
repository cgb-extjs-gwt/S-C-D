using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class WhereSqlHelper : SqlHelper, IWhereSqlHelper<ISqlBuilder>
    {
        public WhereSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public ISqlBuilder Where(ISqlBuilder condition)
        {
            return new WhereSqlBuilder
            {
                Condition = condition,
                SqlBuilder = this.ToSqlBuilder()
            };
        }

        public ISqlBuilder Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public ISqlBuilder Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null) 
        {
            ISqlBuilder result;

            if (filter == null || filter.Count == 0)
            {
                result = this.ToSqlBuilder();
            }
            else
            {
                var inBuilders = this.HandleFilter(filter, tableName);

                result = new WhereSqlBuilder
                {
                    SqlBuilder = this.ToSqlBuilder(),
                    Condition = ConditionHelper.And(inBuilders).ToSqlBuilder()
                };
            }

            return result;
        }

        private List<InSqlBuilder> HandleFilter(IDictionary<string, IEnumerable<object>> filter, string tableName)
        {
            var inBuilders = new List<InSqlBuilder>();

            foreach (var filterItem in filter)
            {
                var parameterBuilders = new List<ParameterSqlBuilder>();
                var index = 0;

                foreach(var value in filterItem.Value)
                {
                    parameterBuilders.Add(new ParameterSqlBuilder
                    {
                        ParamInfo = value as CommandParameterInfo ?? new CommandParameterInfo
                        {
                            Name = $"{filterItem.Key}_{index++}",
                            Value = value
                        }
                    });
                }

                if (parameterBuilders.Count > 0)
                {
                    inBuilders.Add(new InSqlBuilder
                    {
                        Table = tableName,
                        Column = filterItem.Key,
                        Values = parameterBuilders
                    });
                }
            }

            return inBuilders;
        }
    }
}
