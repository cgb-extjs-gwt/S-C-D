using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SqlHelper
    {
        private readonly ISqlBuilder sqlBuilder;

        public SqlHelper(ISqlBuilder sqlBuilder)
        {
            this.sqlBuilder = sqlBuilder;
        }

        public SqlHelper(SqlHelper helper)
            : this(helper.sqlBuilder)
        {
        }

        public string ToSql()
        {
            return this.sqlBuilder.Build(new SqlBuilderContext());
        }

        public ISqlBuilder ToSqlBuilder()
        {
            return this.sqlBuilder;
        }

        public IEnumerable<CommandParameterInfo> GetParameters()
        {
            var paramDictionary = new Dictionary<string, CommandParameterInfo>();

            foreach (var param in this.GetParameters(this.sqlBuilder))
            {
                if (paramDictionary.TryGetValue(param.Name, out var paramDict))
                {
                    if (!object.Equals(param.Value, paramDict.Value))
                    {
                        throw new Exception("There are two parameters that have the same name, but different values.");
                    }
                }
                else
                {
                    paramDictionary.Add(param.Name, param);
                }
            }

            return paramDictionary.Values;
        }

        private IEnumerable<CommandParameterInfo> GetParameters(ISqlBuilder sqlBuilder)
        {
            var paramBuilder = sqlBuilder as ParameterSqlBuilder;
            if (paramBuilder == null)
            {
                foreach (var childBuilder in sqlBuilder.GetChildrenBuilders())
                {
                    foreach (var paramInfo in this.GetParameters(childBuilder))
                    {
                        yield return paramInfo;
                    }
                }
            }
            else
            {
                yield return paramBuilder.ParamInfo;
            }
        }
    }
}
