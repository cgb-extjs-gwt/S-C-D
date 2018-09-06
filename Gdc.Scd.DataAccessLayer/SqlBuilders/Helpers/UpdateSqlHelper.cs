using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UpdateSqlHelper : SqlHelper, IWhereSqlHelper<SqlHelper>, IFromSqlHelper<UpdateCommonSqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly FromSqlHepler fromSqlHelper;

        public UpdateSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
            this.fromSqlHelper = new FromSqlHepler(sqlBuilder);
        }

        public UpdateCommonSqlHelper From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null)
        {
            return new UpdateCommonSqlHelper(this.fromSqlHelper.From(tabeName, schemaName, dataBaseName, alias));
        }

        public UpdateCommonSqlHelper From(BaseEntityMeta meta, string alias = null)
        {
            return new UpdateCommonSqlHelper(this.fromSqlHelper.From(meta, alias));
        }

        public UpdateCommonSqlHelper FromQuery(ISqlBuilder query, string alias = null)
        {
            return new UpdateCommonSqlHelper(this.fromSqlHelper.FromQuery(query, alias));
        }

        public UpdateCommonSqlHelper FromQuery(SqlHelper sqlHelper, string alias = null)
        {
            return new UpdateCommonSqlHelper(this.fromSqlHelper.FromQuery(sqlHelper, alias));
        }

        public SqlHelper Where(ISqlBuilder condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(ConditionHelper condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SqlHelper(this.whereHelper.Where(filter, tableName));
        }
    }
}
