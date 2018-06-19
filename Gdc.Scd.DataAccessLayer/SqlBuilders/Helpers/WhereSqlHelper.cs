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
    public class WhereSqlHelper : SqlHelper
    {
        public WhereSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        //public WhereSqlHelper(ISqlBuilder sqlBuilder, IEnumerable<CommandParameterInfo> parameters)
        //    : base(sqlBuilder, parameters)
        //{
        //}

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
            //return new SqlHelper(
            //    this.GetWhereSqlBuilder(condition.ToSqlBuilder()),
            //    condition.GetParameters()); 

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
                        ParamInfo = new CommandParameterInfo
                        {
                            Name = $"{value}_{index}",
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

        //private WhereSqlBuilder GetWhereSqlBuilder(ISqlBuilder condition)
        //{
        //    return new WhereSqlBuilder
        //    {
        //        Condition = condition,
        //        SqlBuilder = this.ToSqlBuilder()
        //    };
        //}
    }
}
