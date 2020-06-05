using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectFromSqlHelper : UnionSqlHelper, IFromSqlHelper<SelectJoinSqlHelper>
    {
        private readonly FromSqlHepler fromSqlHelper;

        public SelectFromSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.fromSqlHelper = new FromSqlHepler(sqlBuilder);
        }

        SelectJoinSqlHelper IFromSqlHelper<SelectJoinSqlHelper>.From(string tabeName, string schemaName, string dataBaseName, string alias)
        {
            return this.From(tabeName, schemaName, dataBaseName, alias);
        }

        SelectJoinSqlHelper IFromSqlHelper<SelectJoinSqlHelper>.From(BaseEntityMeta meta, string alias)
        {
            return this.From(meta, alias);
        }

        public SelectJoinSqlHelper From(
            string tabeName, 
            string schemaName = null, 
            string dataBaseName = null, 
            string alias = null,
            IsolationLevel? isolationLevel = null)
        {
            var sqlBuilder = this.fromSqlHelper.From(tabeName, schemaName, dataBaseName, alias);

            return this.WrapByIsolationLevel(sqlBuilder, isolationLevel);
        }

        public SelectJoinSqlHelper From(BaseEntityMeta meta, string alias = null, IsolationLevel? isolationLevel = null)
        {
            var sqlBuilder = this.fromSqlHelper.From(meta, alias);

            return this.WrapByIsolationLevel(sqlBuilder, isolationLevel);
        }

        public SelectJoinSqlHelper FromQuery(ISqlBuilder query, string alias = "t")
        {
            return new SelectJoinSqlHelper(this.fromSqlHelper.FromQuery(query, alias));
        }

        public SelectJoinSqlHelper FromQuery(SqlHelper sqlHelper, string alias = null)
        {
            return new SelectJoinSqlHelper(this.fromSqlHelper.FromQuery(sqlHelper, alias));
        }

        public SelectJoinSqlHelper FromFunction(
            string schema, 
            string func, 
            IEnumerable<ISqlBuilder> parameters = null, 
            string alias = "t")
        {
            return new SelectJoinSqlHelper(new FromSqlBuilder
            {
                Query = this.fromSqlHelper.ToSqlBuilder(),
                From = new AliasSqlBuilder
                {
                    Alias = alias,
                    Query = new InvokeFuncSqlBuilder
                    {
                        Shema = schema,
                        Func = func,
                        Params = parameters
                    }
                } 
            });
        }

        public SelectJoinSqlHelper FromFunctionWithParams(
            string schema,
            string func,
            IEnumerable<object> values,
            string alias = "t")
        {
            var parameters = values.Select(value => new ParameterSqlBuilder(value)).ToArray();

            return this.FromFunction(schema, func, parameters, alias);
        }

        private SelectJoinSqlHelper WrapByIsolationLevel(ISqlBuilder sqlBuilder, IsolationLevel? isolationLevel = null)
        {
            sqlBuilder = isolationLevel.HasValue
                ? new WithIsolationLevelSqlBuilder
                {
                    Query = sqlBuilder,
                    IsolationLevel = isolationLevel.Value
                }
                : sqlBuilder;

            return new SelectJoinSqlHelper(sqlBuilder);
        }
    }
}
