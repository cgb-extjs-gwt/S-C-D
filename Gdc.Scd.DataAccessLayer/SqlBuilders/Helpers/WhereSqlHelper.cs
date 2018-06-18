using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Data.Common;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class WhereSqlHelper : BaseSqlHelper
    {
        public WhereSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public WhereSqlBuilder Where(ISqlBuilder condition)
        {
            return new WhereSqlBuilder
            {
                Condition = condition,
                SqlBuilder = this.ToSqlBuilder()
            };
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
                var (inBuilders, parameters) = this.HandleFilter(filter, tableName);

                this.Parameters.AddRange(parameters);

                result = new WhereSqlBuilder
                {
                    SqlBuilder = this.ToSqlBuilder(),
                    Condition = ConditionHelper.And(inBuilders).ToSqlBuilder()
                };
            }

            return result;
        }

        private (List<InSqlBuilder> inBuilders, List<CommandParameterInfo> parameters) HandleFilter(IDictionary<string, IEnumerable<object>> filter, string tableName)
        {
            var inBuilders = new List<InSqlBuilder>();
            var parameters = new List<CommandParameterInfo>();

            foreach (var filterItem in filter)
            {
                var parameterBuilders = new List<ParameterSqlBuilder>();
                var index = 0;

                foreach(var value in filterItem.Value)
                {
                    var paramName = $"{value}_{index}";

                    parameters.Add(new CommandParameterInfo
                    {
                        Name = paramName,
                        Value = value
                    });

                    parameterBuilders.Add(new ParameterSqlBuilder
                    {
                        Name = paramName
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

            return (inBuilders, parameters);
        }
    }
}
