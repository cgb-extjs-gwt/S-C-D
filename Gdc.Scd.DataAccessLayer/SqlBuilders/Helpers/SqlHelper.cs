using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SqlHelper
    {
        private readonly ISqlBuilder sqlBuilder;

        //protected List<CommandParameterInfo> Parameters { get; private set; }

        public SqlHelper(ISqlBuilder sqlBuilder)
        {
            this.sqlBuilder = sqlBuilder;
        }

        //public SqlHelper(ISqlBuilder sqlBuilder, IEnumerable<CommandParameterInfo> parameters)
        //{
        //    this.sqlBuilder = sqlBuilder;
        //    this.Parameters = parameters.ToList();
        //}

        public SqlHelper(SqlHelper helper)
            : this(helper.sqlBuilder)
        {
        }

        //public static T Create<T>(ISqlBuilder sqlBuilder, IEnumerable<CommandParameterInfo> parameters) 
        //    where T : SqlHelper, new()
        //{
        //    var helper = new T();
        //    helper.sqlBuilder = sqlBuilder;
        //    helper.Parameters = parameters.ToList();

        //    return helper;
        //}

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
            return this.GetParameters(this.sqlBuilder);
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

        //public IEnumerable<CommandParameterInfo> GetParameters()
        //{
        //    return this.Parameters.AsReadOnly();
        //}
    }
}
